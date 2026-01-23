namespace SHNGearBE.Models.DTOs.Product;

public class ProductVariantRequest
{
    public Guid? Id { get; set; }
    public string Sku { get; set; } = null!;
    public string? Name { get; set; }
    public int Quantity { get; set; }
    public int SafetyStock { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public Dictionary<Guid, string>? Attributes { get; set; }
}
