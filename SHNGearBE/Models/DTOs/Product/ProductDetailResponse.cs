namespace SHNGearBE.Models.DTOs.Product;

public class ProductDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = null!;

    public List<string> ImageUrls { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<ProductAttributeResponse> Attributes { get; set; } = new();
    public List<ProductVariantResponse> Variants { get; set; } = new();
}

public class ProductAttributeResponse
{
    public Guid AttributeDefinitionId { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}
