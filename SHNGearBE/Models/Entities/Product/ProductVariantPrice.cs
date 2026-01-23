using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class ProductVariantPrice : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal BasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
