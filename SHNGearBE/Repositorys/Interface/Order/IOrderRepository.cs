using SHNGearBE.Models.Entities.Order;
using SHNGearBE.Models.Enums;
using SHNGearBE.Repositorys.Interface;

namespace SHNGearBE.Repositorys.Interface.Order;

public interface IOrderRepository : IGenericRepository<Models.Entities.Order.Order>
{
    Task<Models.Entities.Order.Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Models.Entities.Order.Order?> GetByIdAndAccountAsync(Guid orderId, Guid accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.Entities.Order.Order>> GetByAccountAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Models.Entities.Order.Order>> GetPagedAsync(OrderStatus? status, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(OrderStatus? status, CancellationToken cancellationToken = default);
}
