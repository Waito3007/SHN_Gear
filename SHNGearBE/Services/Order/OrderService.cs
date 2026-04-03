using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Infrastructure.Payment;
using SHNGearBE.Models.DTOs.Common;
using SHNGearBE.Models.DTOs.Order;
using SHNGearBE.Models.Enums;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Address;
using SHNGearBE.Repositorys.Interface.Account;
using SHNGearBE.Repositorys.Interface.Order;
using SHNGearBE.Services.Interfaces.Cart;
using SHNGearBE.Services.Interfaces.Order;
using SHNGearBE.UnitOfWork;
using SHNGearMailService.Abstractions;
using SHNGearMailService.Models;
using System.Globalization;
using OrderEntity = SHNGearBE.Models.Entities.Order.Order;
using OrderItemEntity = SHNGearBE.Models.Entities.Order.OrderItem;

namespace SHNGearBE.Services.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly ICartService _cartService;
    private readonly IPaymentStrategyResolver _paymentStrategyResolver;
    private readonly IPayPalGatewayService _payPalGatewayService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService<OrderService> _logService;

    public OrderService(
        IOrderRepository orderRepository,
        IAccountRepository accountRepository,
        IAddressRepository addressRepository,
        ICartService cartService,
        IPaymentStrategyResolver paymentStrategyResolver,
        IPayPalGatewayService payPalGatewayService,
        IEmailService emailService,
        IEmailTemplateRenderer emailTemplateRenderer,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogService<OrderService> logService)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _addressRepository = addressRepository;
        _cartService = cartService;
        _paymentStrategyResolver = paymentStrategyResolver;
        _payPalGatewayService = payPalGatewayService;
        _emailService = emailService;
        _emailTemplateRenderer = emailTemplateRenderer;
        _context = context;
        _unitOfWork = unitOfWork;
        _logService = logService;
    }

    public async Task<CreatePayPalOrderResponse> CreatePayPalOrderAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var cart = await _cartService.GetCartAsync(accountId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new ProjectException(ResponseType.BadRequest, "Gio hang trong");
        }

        var referenceId = accountId.ToString("N", CultureInfo.InvariantCulture);
        var createOrderResult = await _payPalGatewayService.CreateOrderAsync(cart.TotalAmount, referenceId, cancellationToken);

        return new CreatePayPalOrderResponse
        {
            OrderId = createOrderResult.OrderId,
            CurrencyCode = createOrderResult.CurrencyCode,
            AmountUsd = createOrderResult.AmountUsd,
            AppliedVndPerUsdRate = createOrderResult.AppliedVndPerUsdRate,
            UsedFallbackRate = createOrderResult.UsedFallbackRate
        };
    }

    public Task<PayPalClientConfigResponse> GetPayPalClientConfigAsync(CancellationToken cancellationToken = default)
    {
        var response = new PayPalClientConfigResponse
        {
            ClientId = _payPalGatewayService.GetClientId(),
            CurrencyCode = _payPalGatewayService.GetCurrencyCode()
        };

        return Task.FromResult(response);
    }

    public async Task ProcessPayPalWebhookAsync(string eventId, string eventType, string? captureId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        var duplicateEvent = await _context.WebhookEvents
            .AnyAsync(e => !e.IsDelete && e.Provider == "PayPal" && e.EventId == eventId, cancellationToken);

        if (duplicateEvent)
        {
            await _logService.WriteMessageAsync($"PayPal webhook ignored as duplicate: {eventId}");
            return;
        }

        var order = await _context.Orders
            .Where(o => !o.IsDelete && o.PaymentProvider == PaymentProvider.PayPal && o.PaymentTransactionId == captureId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order == null)
        {
            await _logService.WriteMessageAsync($"PayPal webhook ignored: no order for capture {captureId}");

            _context.WebhookEvents.Add(new Models.Entities.Order.WebhookEvent
            {
                Id = Guid.NewGuid(),
                Provider = "PayPal",
                EventId = eventId,
                EventType = eventType,
                CaptureId = captureId,
                ProcessedAt = DateTime.UtcNow,
                OrderId = null
            });

            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException)
            {
                // Ignore duplicate persistence race between webhook retries.
            }
            return;
        }

        if (string.Equals(eventType, "PAYMENT.CAPTURE.COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            order.PaymentStatus = PaymentStatus.Completed;
            order.PaidAt ??= DateTime.UtcNow;
        }
        else if (string.Equals(eventType, "PAYMENT.CAPTURE.REFUNDED", StringComparison.OrdinalIgnoreCase))
        {
            order.PaymentStatus = PaymentStatus.Refunded;
        }
        else if (string.Equals(eventType, "PAYMENT.CAPTURE.DENIED", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(eventType, "PAYMENT.CAPTURE.DECLINED", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(eventType, "PAYMENT.CAPTURE.REVERSED", StringComparison.OrdinalIgnoreCase))
        {
            order.PaymentStatus = PaymentStatus.Failed;
        }
        else
        {
            return;
        }

        order.UpdateAt = DateTime.UtcNow;

        _context.WebhookEvents.Add(new Models.Entities.Order.WebhookEvent
        {
            Id = Guid.NewGuid(),
            Provider = "PayPal",
            EventId = eventId,
            EventType = eventType,
            CaptureId = captureId,
            ProcessedAt = DateTime.UtcNow,
            OrderId = order.Id
        });

        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (DbUpdateException)
        {
            // Duplicate persistence from concurrent webhook retries can be safely ignored.
        }
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

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new ProjectException(ResponseType.InvalidData, "Idempotency key is required");
        }

        var normalizedIdempotencyKey = request.IdempotencyKey.Trim();
        var existingOrder = await _orderRepository.GetByIdempotencyKeyAsync(accountId, normalizedIdempotencyKey, cancellationToken);
        if (existingOrder != null)
        {
            return MapToResponse(existingOrder);
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
                IdempotencyKey = normalizedIdempotencyKey,
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

            await TrySendOrderPlacedEmailAsync(accountId, order, address, cancellationToken);

            return MapToResponse(order);
        }
        catch (DbUpdateException)
        {
            await _unitOfWork.RollbackAsync();

            var duplicateOrder = await _orderRepository.GetByIdempotencyKeyAsync(accountId, normalizedIdempotencyKey, cancellationToken);
            if (duplicateOrder != null)
            {
                await _logService.WriteMessageAsync($"Order create request replayed: {duplicateOrder.Id} - Account: {accountId}");
                return MapToResponse(duplicateOrder);
            }

            throw;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task TrySendOrderPlacedEmailAsync(Guid accountId, OrderEntity order, SHNGearBE.Models.Entities.Account.Address deliveryAddress, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountWithDetailsAsync(accountId);
            if (account == null || string.IsNullOrWhiteSpace(account.Email))
            {
                return;
            }

            var recipientName = account.AccountDetail?.Name
                                ?? account.AccountDetail?.FirstName
                                ?? account.Username
                                ?? account.Email;

            var template = _emailTemplateRenderer.RenderOrderPlacedTemplate(new OrderPlacedEmailTemplateData
            {
                AppName = "SHNGear",
                RecipientName = recipientName,
                OrderCode = order.Code,
                CreatedAtUtc = order.CreateAt,
                PaymentMethod = order.PaymentProvider.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                CurrencyCode = "VND",
                DeliveryAddress = $"{deliveryAddress.Street}, {deliveryAddress.Ward}, {deliveryAddress.District}, {deliveryAddress.Province}",
                SupportEmail = "shngearvn@gmail.com",
                Items = order.Items.Select(i => new OrderPlacedEmailItem
                {
                    ProductName = i.ProductNameSnapshot,
                    VariantName = i.VariantNameSnapshot,
                    Sku = i.SkuSnapshot,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    SubTotal = i.SubTotal
                }).ToList()
            });

            var sendResult = await _emailService.SendAsync(new EmailMessage
            {
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true,
                To =
                {
                    new EmailAddress
                    {
                        Address = account.Email,
                        DisplayName = recipientName
                    }
                }
            }, cancellationToken);

            if (!sendResult.Success)
            {
                await _logService.WriteMessageAsync($"Order email send failed for order {order.Id}: {sendResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
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
        RefundSummary? refundSummary = null;
        try
        {
            await RestoreStockAsync(order, cancellationToken);

            var shouldAutoRefund = ShouldAutoRefundOnCancel(order.Status);
            refundSummary = await RefundPayPalIfNeededAsync(order, shouldAutoRefund, null, reason, cancellationToken);

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            order.CancelledReason = string.IsNullOrWhiteSpace(reason) ? "Khach hang huy don" : reason.Trim();
            order.UpdateAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            await _logService.WriteMessageAsync($"Order cancelled by customer: {order.Id} - Account: {accountId}");
            if (refundSummary != null)
            {
                await TrySendRefundCompletedEmailAsync(accountId, order, refundSummary, reason, cancellationToken);
            }

            return MapToResponse(order);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponse> ApproveRefundAsync(Guid orderId, decimal? amountUsd, string? reason, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Don hang khong ton tai");
        }

        if (order.Status != OrderStatus.Cancelled)
        {
            throw new ProjectException(ResponseType.BadRequest, "Chi duoc duyet hoan tien cho don da huy");
        }

        if (order.PaymentProvider != PaymentProvider.PayPal)
        {
            throw new ProjectException(ResponseType.BadRequest, "Don hang khong su dung PayPal");
        }

        if (order.PaymentStatus == PaymentStatus.Refunded)
        {
            throw new ProjectException(ResponseType.BadRequest, "Don hang da hoan tien truoc do");
        }

        if (order.PaymentStatus != PaymentStatus.Completed)
        {
            throw new ProjectException(ResponseType.BadRequest, "Don hang chua thanh toan, khong the hoan tien");
        }

        var refundSummary = await RefundPayPalIfNeededAsync(order, true, amountUsd, reason, cancellationToken);

        if (!string.IsNullOrWhiteSpace(reason))
        {
            var normalizedReason = reason.Trim();
            order.CancelledReason = string.IsNullOrWhiteSpace(order.CancelledReason)
                ? normalizedReason
                : $"{order.CancelledReason} | Refund note: {normalizedReason}";
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveAsync();

        await _logService.WriteMessageAsync($"Order refund approved by admin: {order.Id} - AmountUsd: {(amountUsd.HasValue ? amountUsd.Value.ToString("0.00", CultureInfo.InvariantCulture) : "FULL")}");
        if (refundSummary != null)
        {
            await TrySendRefundCompletedEmailAsync(order.AccountId, order, refundSummary, reason, cancellationToken);
        }

        return MapToResponse(order);
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

        if (order.Status == status)
        {
            return MapToResponse(order);
        }

        if (!CanTransition(order.Status, status))
        {
            throw new ProjectException(ResponseType.BadRequest, "Khong the cap nhat trang thai theo luong hien tai");
        }

        if (status == OrderStatus.Cancelled)
        {
            await _unitOfWork.BeginTransactionAsync();
            RefundSummary? refundSummary = null;
            try
            {
                await RestoreStockAsync(order, cancellationToken);

                var shouldAutoRefund = ShouldAutoRefundOnCancel(order.Status);
                refundSummary = await RefundPayPalIfNeededAsync(order, shouldAutoRefund, null, order.CancelledReason, cancellationToken);

                order.Status = OrderStatus.Cancelled;
                order.UpdateAt = DateTime.UtcNow;
                order.CancelledAt = DateTime.UtcNow;
                order.CancelledReason ??= "Admin huy don";

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            await _logService.WriteMessageAsync($"Order status updated: {order.Id} -> {status}");
            if (refundSummary != null)
            {
                await TrySendRefundCompletedEmailAsync(order.AccountId, order, refundSummary, order.CancelledReason, cancellationToken);
            }

            return MapToResponse(order);
        }

        order.Status = status;
        order.UpdateAt = DateTime.UtcNow;

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

    private static bool ShouldAutoRefundOnCancel(OrderStatus status)
    {
        return status == OrderStatus.Pending;
    }

    private async Task RestoreStockAsync(OrderEntity order, CancellationToken cancellationToken)
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
    }

    private async Task<RefundSummary?> RefundPayPalIfNeededAsync(OrderEntity order, bool shouldRefund, decimal? amountUsd, string? reason, CancellationToken cancellationToken)
    {
        if (!shouldRefund)
        {
            return null;
        }

        if (order.PaymentProvider != PaymentProvider.PayPal || order.PaymentStatus != PaymentStatus.Completed)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(order.PaymentTransactionId))
        {
            throw new ProjectException(ResponseType.BadRequest, "Khong tim thay ma giao dich PayPal de hoan tien");
        }

        if (amountUsd.HasValue && amountUsd.Value <= 0)
        {
            throw new ProjectException(ResponseType.BadRequest, "So tien hoan phai lon hon 0");
        }

        var captureAmountResult = await _payPalGatewayService.GetCaptureAmountAsync(order.PaymentTransactionId, cancellationToken);
        if (!captureAmountResult.Success || !captureAmountResult.AmountUsd.HasValue)
        {
            throw new ProjectException(ResponseType.BadRequest, captureAmountResult.ErrorMessage ?? "Khong lay duoc tong so tien giao dich PayPal");
        }

        var alreadyRefundedAmount = await _context.RefundRecords
            .Where(r => !r.IsDelete && r.OrderId == order.Id)
            .SumAsync(r => r.RefundAmountUsd, cancellationToken);

        var remainingRefundableAmount = captureAmountResult.AmountUsd.Value - alreadyRefundedAmount;
        if (remainingRefundableAmount <= 0)
        {
            throw new ProjectException(ResponseType.BadRequest, "Don hang da duoc hoan tien toi da");
        }

        var requestedRefundAmount = amountUsd ?? remainingRefundableAmount;
        if (requestedRefundAmount <= 0)
        {
            throw new ProjectException(ResponseType.BadRequest, "So tien hoan phai lon hon 0");
        }

        if (requestedRefundAmount > remainingRefundableAmount)
        {
            throw new ProjectException(ResponseType.BadRequest, "So tien hoan vuot qua so tien con lai co the hoan");
        }

        var refundResult = await _payPalGatewayService.RefundCaptureAsync(order.PaymentTransactionId, requestedRefundAmount, cancellationToken);
        if (!refundResult.Success)
        {
            throw new ProjectException(ResponseType.BadRequest, refundResult.ErrorMessage ?? "Hoan tien PayPal that bai");
        }

        if (string.IsNullOrWhiteSpace(refundResult.RefundId))
        {
            throw new ProjectException(ResponseType.BadRequest, "PayPal khong tra ve ma giao dich refund");
        }

        var refundedAmount = refundResult.RefundedAmountUsd ?? requestedRefundAmount;
        var totalCapturedAmount = captureAmountResult.AmountUsd.Value;
        var totalRefundedAmount = alreadyRefundedAmount + refundedAmount;
        if (totalRefundedAmount >= totalCapturedAmount)
        {
            order.PaymentStatus = PaymentStatus.Refunded;
        }

        _context.RefundRecords.Add(new Models.Entities.Order.RefundRecord
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            RefundTransactionId = refundResult.RefundId,
            CaptureTransactionId = order.PaymentTransactionId,
            RefundAmountUsd = refundedAmount,
            TotalCapturedAmountUsd = totalCapturedAmount,
            CurrencyCode = captureAmountResult.CurrencyCode,
            RefundReason = string.IsNullOrWhiteSpace(reason) ? order.CancelledReason : reason.Trim(),
            RefundedAt = DateTime.UtcNow
        });

        order.UpdateAt = DateTime.UtcNow;
        return new RefundSummary(refundedAmount, totalCapturedAmount, captureAmountResult.CurrencyCode);
    }

    private async Task TrySendRefundCompletedEmailAsync(Guid accountId, OrderEntity order, RefundSummary refundSummary, string? reason, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetAccountWithDetailsAsync(accountId);
            if (account == null || string.IsNullOrWhiteSpace(account.Email))
            {
                return;
            }

            var recipientName = account.AccountDetail?.Name
                                ?? account.AccountDetail?.FirstName
                                ?? account.Username
                                ?? account.Email;

            var template = _emailTemplateRenderer.RenderRefundCompletedTemplate(new RefundCompletedEmailTemplateData
            {
                AppName = "SHNGear",
                RecipientName = recipientName,
                OrderCode = order.Code,
                RefundedAtUtc = DateTime.UtcNow,
                RefundedAmount = refundSummary.RefundedAmountUsd,
                TotalCapturedAmount = refundSummary.TotalCapturedAmountUsd,
                CurrencyCode = refundSummary.CurrencyCode,
                RefundReason = string.IsNullOrWhiteSpace(reason) ? order.CancelledReason : reason,
                SupportEmail = "shngearvn@gmail.com"
            });

            var sendResult = await _emailService.SendAsync(new EmailMessage
            {
                Subject = template.Subject,
                Body = template.HtmlBody,
                IsHtml = true,
                To =
                {
                    new EmailAddress
                    {
                        Address = account.Email,
                        DisplayName = recipientName
                    }
                }
            }, cancellationToken);

            if (!sendResult.Success)
            {
                await _logService.WriteMessageAsync($"Refund email send failed for order {order.Id}: {sendResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
        }
    }

    private sealed record RefundSummary(decimal RefundedAmountUsd, decimal TotalCapturedAmountUsd, string CurrencyCode);

    private static bool CanTransition(OrderStatus from, OrderStatus to)
    {
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
