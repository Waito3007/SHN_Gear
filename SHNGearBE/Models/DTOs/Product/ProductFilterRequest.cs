namespace SHNGearBE.Models.DTOs.Product;

public class ProductFilterRequest
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
