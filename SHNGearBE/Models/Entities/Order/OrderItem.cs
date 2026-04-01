using ProductVariantEntity = SHNGearBE.Models.Entities.Product.ProductVariant;

namespace SHNGearBE.Models.Entities.Order;

public class OrderItem : BaseEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }

    public string ProductNameSnapshot { get; set; } = null!;
    public string VariantNameSnapshot { get; set; } = null!;
    public string SkuSnapshot { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual ProductVariantEntity ProductVariant { get; set; } = null!;
}
