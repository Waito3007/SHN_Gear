namespace SHNGearMailService.Models;

public sealed class RefundCompletedEmailTemplateData
{
    public string AppName { get; set; } = "SHNGear";
    public string RecipientName { get; set; } = "Customer";
    public string OrderCode { get; set; } = string.Empty;
    public DateTime RefundedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal RefundedAmount { get; set; }
    public decimal TotalCapturedAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? RefundReason { get; set; }
    public string SupportEmail { get; set; } = "support@shngear.com";
}
