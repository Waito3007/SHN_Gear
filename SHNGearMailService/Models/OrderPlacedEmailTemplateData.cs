namespace SHNGearMailService.Models;

public sealed class OrderPlacedEmailTemplateData
{
    public string AppName { get; set; } = "SHNGear";
    public string RecipientName { get; set; } = "Customer";
    public string OrderCode { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string PaymentMethod { get; set; } = "COD";
    public string PaymentStatus { get; set; } = "Pending";
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "VND";
    public string? DeliveryAddress { get; set; }
    public string SupportEmail { get; set; } = "support@shngear.com";
    public List<OrderPlacedEmailItem> Items { get; set; } = new();
}

public sealed class OrderPlacedEmailItem
{
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}