namespace SHNGearBE.Models.DTOs.Product;

public class UpdateProductRequest
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public List<string>? ImageUrls { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<Guid, string>? Attributes { get; set; }
    public List<ProductVariantRequest> Variants { get; set; } = new();
}
