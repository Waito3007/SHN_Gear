using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Order;

public class RefundRecord : BaseEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string RefundTransactionId { get; set; } = null!;
    public string CaptureTransactionId { get; set; } = null!;
    public decimal RefundAmountUsd { get; set; }
    public decimal TotalCapturedAmountUsd { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? RefundReason { get; set; }
    public DateTime RefundedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
