using SHNGearBE.Models.DTOs.Account;

namespace SHNGearBE.Services.Interfaces.Account;

public interface IAccountService
{
    Task<AccountDto?> GetAccountByIdAsync(Guid accountId);
    Task<AccountDto?> GetAccountByEmailAsync(string email);
    Task<AccountDto?> UpdateAccountAsync(Guid accountId, UpdateAccountRequestDto request);
    Task<bool> DeleteAccountAsync(Guid accountId);
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
    Task<bool> AssignRoleToAccountAsync(Guid accountId, Guid roleId);
    Task<bool> RemoveRoleFromAccountAsync(Guid accountId, Guid roleId);
    Task<IEnumerable<string>> GetAccountPermissionsAsync(Guid accountId);
}
