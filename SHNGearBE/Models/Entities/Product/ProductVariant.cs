using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class ProductVariant : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string? Name { get; set; }

    public int Quantity { get; set; }
    public int SafetyStock { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<ProductVariantPrice> Prices { get; set; } = new List<ProductVariantPrice>();
    public virtual ICollection<ProductVariantAttribute> VariantAttributes { get; set; } = new List<ProductVariantAttribute>();
}
