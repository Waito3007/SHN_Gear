using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Repositorys.Interface.Address;
using AddressEntity = SHNGearBE.Models.Entities.Account.Address;

namespace SHNGearBE.Repositorys.Address;

public class AddressRepository : GenericRepository<AddressEntity>, IAddressRepository
{
    public AddressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AddressEntity>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => !a.IsDelete && a.AccountId == accountId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreateAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<AddressEntity?> GetByIdAndAccountAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => !a.IsDelete && a.Id == id && a.AccountId == accountId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AddressEntity?> GetDefaultByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => !a.IsDelete && a.AccountId == accountId && a.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task ClearDefaultAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var defaults = await _dbSet
            .Where(a => !a.IsDelete && a.AccountId == accountId && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var addr in defaults)
        {
            addr.IsDefault = false;
            addr.UpdateAt = DateTime.UtcNow;
        }
    }

    public async Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => !a.IsDelete && a.AccountId == accountId)
            .CountAsync(cancellationToken);
    }
}
