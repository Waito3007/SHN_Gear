using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.DTOs.Account;
using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Account;
using SHNGearBE.Repositorys.Interface.Permission;
using SHNGearBE.Services.Interfaces.Account;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Services.Account;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(
        IAccountRepository accountRepository,
        IPermissionRepository permissionRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _permissionRepository = permissionRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid accountId)
    {
        var account = await _accountRepository.GetAccountWithRolesAndPermissionsAsync(accountId);
        if (account == null)
        {
            return null;
        }

        return MapToAccountDto(account);
    }

    public async Task<AccountDto?> GetAccountByEmailAsync(string email)
    {
        var account = await _accountRepository.GetByEmailAsync(email);
        if (account == null)
        {
            return null;
        }

        account = await _accountRepository.GetAccountWithRolesAndPermissionsAsync(account.Id);
        return account == null ? null : MapToAccountDto(account);
    }

    public async Task<AccountDto?> UpdateAccountAsync(Guid accountId, UpdateAccountRequestDto request)
    {
        var account = await _accountRepository.GetAccountWithDetailsAsync(accountId);
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        // Update account
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (await _accountRepository.UsernameExistsAsync(request.Username))
            {
                var existingAccount = await _accountRepository.GetByUsernameAsync(request.Username);
                if (existingAccount != null && existingAccount.Id != accountId)
                {
                    throw new ProjectException(ResponseType.AlreadyExists, "Username already exists");
                }
            }
            account.Username = request.Username;
        }

        // Update account details
        if (account.AccountDetail != null)
        {
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                account.AccountDetail.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                account.AccountDetail.Name = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                account.AccountDetail.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.Address))
                account.AccountDetail.Address = request.Address;

            account.AccountDetail.UpdateAt = DateTime.UtcNow;
        }

        account.UpdateAt = DateTime.UtcNow;
        await _accountRepository.UpdateAsync(account);
        await _unitOfWork.SaveAsync();

        return await GetAccountByIdAsync(accountId);
    }

    public async Task<bool> DeleteAccountAsync(Guid accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            return false;
        }

        await _accountRepository.DeleteAsync(accountId);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _context.Accounts
            .Include(a => a.AccountDetail)
            .Include(a => a.AccountRoles)
                .ThenInclude(ar => ar.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Where(a => !a.IsDelete)
            .ToListAsync();

        return accounts.Select(MapToAccountDto).ToList();
    }

    public async Task<bool> AssignRoleToAccountAsync(Guid accountId, Guid roleId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        var existingRole = await _context.AccountRoles
            .FirstOrDefaultAsync(ar => ar.AccountId == accountId && ar.RoleId == roleId);

        if (existingRole != null)
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Account already has this role");
        }

        var accountRole = new AccountRole
        {
            AccountId = accountId,
            RoleId = roleId
        };

        _context.AccountRoles.Add(accountRole);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> RemoveRoleFromAccountAsync(Guid accountId, Guid roleId)
    {
        var accountRole = await _context.AccountRoles
            .FirstOrDefaultAsync(ar => ar.AccountId == accountId && ar.RoleId == roleId);

        if (accountRole == null)
        {
            return false;
        }

        _context.AccountRoles.Remove(accountRole);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetAccountPermissionsAsync(Guid accountId)
    {
        return await _permissionRepository.GetPermissionNamesByAccountIdQueryable(accountId).ToListAsync();
    }

    private AccountDto MapToAccountDto(Models.Entities.Account.Account account)
    {
        var roles = account.AccountRoles?.Select(ar => ar.Role.Name).ToList() ?? new List<string>();
        var permissions = account.AccountRoles?
            .SelectMany(ar => ar.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList() ?? new List<string>();

        return new AccountDto
        {
            Id = account.Id,
            Username = account.Username,
            Email = account.Email,
            FirstName = account.AccountDetail?.FirstName,
            LastName = account.AccountDetail?.Name,
            PhoneNumber = account.AccountDetail?.PhoneNumber,
            Address = account.AccountDetail?.Address,
            Roles = roles,
            Permissions = permissions
        };
    }
}
