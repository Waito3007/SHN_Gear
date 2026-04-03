using SHNGearBE.Models.DTOs.Common;
using SHNGearBE.Models.DTOs.Order;
using SHNGearBE.Models.Enums;

namespace SHNGearBE.Services.Interfaces.Order;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(Guid accountId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<CreatePayPalOrderResponse> CreatePayPalOrderAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<PayPalClientConfigResponse> GetPayPalClientConfigAsync(CancellationToken cancellationToken = default);
    Task ProcessPayPalWebhookAsync(string eventId, string eventType, string? captureId, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetMyOrderByIdAsync(Guid accountId, Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderResponse> CancelMyOrderAsync(Guid accountId, Guid orderId, string? reason, CancellationToken cancellationToken = default);
    Task<OrderResponse> ApproveRefundAsync(Guid orderId, decimal? amountUsd, string? reason, CancellationToken cancellationToken = default);

    Task<OrderResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderResponse>> GetOrdersAsync(OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
