using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Enums;
using AccountEntity = SHNGearBE.Models.Entities.Account.Account;

namespace SHNGearBE.Models.Entities.Order;

public class Order : BaseEntity
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid DeliveryAddressId { get; set; }

    public string Code { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.Cod;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Note { get; set; }
    public string? PaymentTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledReason { get; set; }

    public virtual AccountEntity Account { get; set; } = null!;
    public virtual Address DeliveryAddress { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
