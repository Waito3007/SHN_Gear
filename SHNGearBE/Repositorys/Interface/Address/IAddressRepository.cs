using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Repositorys.Interface;

namespace SHNGearBE.Repositorys.Interface.Address;

public interface IAddressRepository : IGenericRepository<Models.Entities.Account.Address>
{
    Task<IReadOnlyList<Models.Entities.Account.Address>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<Models.Entities.Account.Address?> GetByIdAndAccountAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default);
    Task<Models.Entities.Account.Address?> GetDefaultByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task ClearDefaultAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
}
