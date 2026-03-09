using AccountEntity = SHNGearBE.Models.Entities.Account.Account;

namespace SHNGearBE.Repositorys.Interface.Account;

public interface IAccountRepository : IGenericRepository<AccountEntity>
{
    Task<AccountEntity?> GetByEmailAsync(string email);
    Task<AccountEntity?> GetByUsernameAsync(string username);
    Task<AccountEntity?> GetAccountWithRolesAndPermissionsAsync(Guid accountId);
    Task<AccountEntity?> GetAccountWithDetailsAsync(Guid accountId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}