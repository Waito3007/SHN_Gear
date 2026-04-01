using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Infrastructure.Payment;
using SHNGearBE.Models.DTOs.Common;
using SHNGearBE.Models.DTOs.Order;
using SHNGearBE.Models.Enums;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Address;
using SHNGearBE.Repositorys.Interface.Order;
using SHNGearBE.Services.Interfaces.Cart;
using SHNGearBE.Services.Interfaces.Order;
using SHNGearBE.UnitOfWork;
using OrderEntity = SHNGearBE.Models.Entities.Order.Order;
using OrderItemEntity = SHNGearBE.Models.Entities.Order.OrderItem;

namespace SHNGearBE.Services.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICartService _cartService;
    private readonly IPaymentStrategyResolver _paymentStrategyResolver;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService<OrderService> _logService;

    public OrderService(
        IOrderRepository orderRepository,
        IAddressRepository addressRepository,
        ICartService cartService,
        IPaymentStrategyResolver paymentStrategyResolver,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogService<OrderService> logService)
    {
        _orderRepository = orderRepository;
        _addressRepository = addressRepository;
        _cartService = cartService;
        _paymentStrategyResolver = paymentStrategyResolver;
        _context = context;
        _unitOfWork = unitOfWork;
        _logService = logService;
    }

    public async Task<OrderResponse> CreateOrderAsync(Guid accountId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndAccountAsync(request.DeliveryAddressId, accountId, cancellationToken);
        if (address == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Dia chi giao hang khong ton tai");
        }

        var cart = await _cartService.GetCartAsync(accountId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new ProjectException(ResponseType.BadRequest, "Gio hang trong");
        }

        var provider = request.PaymentProvider;
        var strategy = _paymentStrategyResolver.Resolve(provider);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var variantIds = cart.Items.Select(i => i.ProductVariantId).Distinct().ToList();

            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => variantIds.Contains(v.Id) && !v.Product.IsDelete)
                .ToDictionaryAsync(v => v.Id, cancellationToken);

            foreach (var item in cart.Items)
            {
                if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                {
                    throw new ProjectException(ResponseType.NotFound, "San pham khong ton tai");
                }

                if (item.Quantity <= 0 || item.Quantity > variant.AvailableToSell)
                {
                    throw new ProjectException(ResponseType.BadRequest, $"Ton kho khong du cho SKU {variant.Sku}");
                }
            }

            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                Code = GenerateOrderCode(),
                AccountId = accountId,
                DeliveryAddressId = address.Id,
                Status = OrderStatus.Pending,
                PaymentProvider = provider,
                PaymentStatus = PaymentStatus.Pending,
                SubTotal = cart.TotalAmount,
                ShippingFee = 0,
                TotalAmount = cart.TotalAmount,
                Note = request.Note?.Trim()
            };

            foreach (var item in cart.Items)
            {
                var variant = variants[item.ProductVariantId];

                variant.Quantity -= item.Quantity;
                variant.UpdateAt = DateTime.UtcNow;

                order.Items.Add(new OrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductNameSnapshot = item.ProductName,
                    VariantNameSnapshot = item.VariantName,
                    SkuSnapshot = item.Sku,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    SubTotal = item.SubTotal
                });
            }

            var paymentContext = new PaymentContext
            {
                AccountId = accountId,
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentToken = request.PaymentToken
            };

            var paymentResult = await strategy.ProcessAsync(paymentContext, cancellationToken);
            if (!paymentResult.Success)
            {
                throw new ProjectException(ResponseType.BadRequest, paymentResult.ErrorMessage ?? "Thanh toan that bai");
            }

            order.PaymentStatus = paymentResult.PaymentStatus;
            order.PaymentTransactionId = paymentResult.TransactionId;
            order.PaidAt = paymentResult.PaymentStatus == PaymentStatus.Completed ? DateTime.UtcNow : null;

            await _orderRepository.AddAsync(order);
            await _unitOfWork.CommitAsync();

            await _cartService.ClearCartAsync(accountId);
            await _logService.WriteMessageAsync($"Order created: {order.Id} - Account: {accountId}");

            return MapToResponse(order);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse?> GetMyOrderByIdAsync(Guid accountId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAndAccountAsync(orderId, accountId, cancellationToken);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid accountId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        EnsureValidPaging(page, pageSize);

        var skip = (page - 1) * pageSize;
        var orders = await _orderRepository.GetByAccountAsync(accountId, skip, pageSize, cancellationToken);
        var total = await _orderRepository.CountByAccountAsync(accountId, cancellationToken);

        return new PagedResult<OrderResponse>
        {
            Items = orders.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<OrderResponse> CancelMyOrderAsync(Guid accountId, Guid orderId, string? reason, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAndAccountAsync(orderId, accountId, cancellationToken);
        if (order == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Don hang khong ton tai");
        }

        if (!CanCancel(order.Status))
        {
            throw new ProjectException(ResponseType.BadRequest, "Don hang khong the huy o trang thai hien tai");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var variantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();
            var variants = await _context.ProductVariants
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, cancellationToken);

            foreach (var item in order.Items)
            {
                if (variants.TryGetValue(item.ProductVariantId, out var variant))
                {
                    variant.Quantity += item.Quantity;
                    variant.UpdateAt = DateTime.UtcNow;
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledReason = string.IsNullOrWhiteSpace(reason) ? "Khach hang huy don" : reason.Trim();
            order.UpdateAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            await _logService.WriteMessageAsync($"Order cancelled by customer: {order.Id} - Account: {accountId}");
            return MapToResponse(order);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<PagedResult<OrderResponse>> GetOrdersAsync(OrderStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        EnsureValidPaging(page, pageSize);

        var skip = (page - 1) * pageSize;
        var orders = await _orderRepository.GetPagedAsync(status, skip, pageSize, cancellationToken);
        var total = await _orderRepository.CountAsync(status, cancellationToken);

        return new PagedResult<OrderResponse>
        {
            Items = orders.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Don hang khong ton tai");
        }

        if (!CanTransition(order.Status, status))
        {
            throw new ProjectException(ResponseType.BadRequest, "Khong the cap nhat trang thai theo luong hien tai");
        }

        order.Status = status;
        order.UpdateAt = DateTime.UtcNow;

        if (status == OrderStatus.Cancelled)
        {
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledReason ??= "Admin huy don";
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveAsync();

        await _logService.WriteMessageAsync($"Order status updated: {order.Id} -> {status}");
        return MapToResponse(order);
    }

    private static string GenerateOrderCode()
    {
        var now = DateTime.UtcNow;
        return $"ORD{now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
    }

    private static void EnsureValidPaging(int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
        {
            throw new ProjectException(ResponseType.InvalidData, "Page va pageSize phai lon hon 0");
        }
    }

    private static bool CanCancel(OrderStatus status)
    {
        return status == OrderStatus.Pending || status == OrderStatus.Confirmed;
    }

    private static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        if (from == to)
        {
            return true;
        }

        if (from == OrderStatus.Cancelled || from == OrderStatus.Delivered)
        {
            return false;
        }

        return from switch
        {
            OrderStatus.Pending => to is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Confirmed => to is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => to is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => to == OrderStatus.Delivered,
            _ => false
        };
    }

    private static OrderResponse MapToResponse(OrderEntity order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Code = order.Code,
            AccountId = order.AccountId,
            DeliveryAddressId = order.DeliveryAddressId,
            Status = order.Status,
            PaymentProvider = order.PaymentProvider,
            PaymentStatus = order.PaymentStatus,
            SubTotal = order.SubTotal,
            ShippingFee = order.ShippingFee,
            TotalAmount = order.TotalAmount,
            Note = order.Note,
            PaymentTransactionId = order.PaymentTransactionId,
            PaidAt = order.PaidAt,
            CancelledAt = order.CancelledAt,
            CancelledReason = order.CancelledReason,
            CreatedAt = order.CreateAt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductNameSnapshot,
                VariantName = i.VariantNameSnapshot,
                Sku = i.SkuSnapshot,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }
}
