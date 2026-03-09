using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using AccountEntity = SHNGearBE.Models.Entities.Account.Account;
using SHNGearBE.Repositorys.Interface.Account;

namespace SHNGearBE.Repositorys.Account;

public class AccountRepository : GenericRepository<AccountEntity>, IAccountRepository
{
    public AccountRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AccountEntity?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(a => a.AccountDetail)
            .FirstOrDefaultAsync(a => a.Email == email && !a.IsDelete);
    }

    public async Task<AccountEntity?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(a => a.AccountDetail)
            .FirstOrDefaultAsync(a => a.Username == username && !a.IsDelete);
    }

    public async Task<AccountEntity?> GetAccountWithRolesAndPermissionsAsync(Guid accountId)
    {
        return await _dbSet
            .Include(a => a.AccountDetail)
            .Include(a => a.AccountRoles)
                .ThenInclude(ar => ar.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDelete);
    }

    public async Task<AccountEntity?> GetAccountWithDetailsAsync(Guid accountId)
    {
        return await _dbSet
            .Include(a => a.AccountDetail)
            .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDelete);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(a => a.Email == email && !a.IsDelete);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet
            .AnyAsync(a => a.Username == username && !a.IsDelete);
    }
}