using SHNGearBE.Models.Enums;

namespace SHNGearBE.Models.DTOs.Order;

public class CreateOrderRequest
{
    public Guid DeliveryAddressId { get; set; }
    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.Cod;
    public string? PaymentToken { get; set; }
    public string? Note { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

public class OrderResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Guid DeliveryAddressId { get; set; }

    public OrderStatus Status { get; set; }
    public PaymentProvider PaymentProvider { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Note { get; set; }
    public string? PaymentTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public string VariantName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
}
