using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.Enums;
using SHNGearBE.Repositorys.Interface.Order;
using OrderEntity = SHNGearBE.Models.Entities.Order.Order;

namespace SHNGearBE.Repositorys.Order;

public class OrderRepository : GenericRepository<OrderEntity>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<OrderEntity?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .Where(o => !o.IsDelete && o.Id == orderId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OrderEntity?> GetByIdAndAccountAsync(Guid orderId, Guid accountId, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .Where(o => !o.IsDelete && o.Id == orderId && o.AccountId == accountId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderEntity>> GetByAccountAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .Where(o => !o.IsDelete && o.AccountId == accountId)
            .OrderByDescending(o => o.CreateAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => !o.IsDelete && o.AccountId == accountId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrderEntity>> GetPagedAsync(OrderStatus? status, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = QueryWithDetails()
            .Where(o => !o.IsDelete);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreateAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(OrderStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(o => !o.IsDelete);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<OrderEntity> QueryWithDetails()
    {
        return _dbSet
            .Include(o => o.Items)
            .Include(o => o.DeliveryAddress);
    }
}
