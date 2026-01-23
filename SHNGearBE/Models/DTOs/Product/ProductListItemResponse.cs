namespace SHNGearBE.Models.DTOs.Product;

public class ProductListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string Currency { get; set; } = "USD";
}
