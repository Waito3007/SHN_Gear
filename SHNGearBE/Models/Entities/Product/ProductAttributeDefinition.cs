using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class ProductAttributeDefinition : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public AttributeDataType DataType { get; set; }

    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    public virtual ICollection<ProductVariantAttribute> ProductVariantAttributes { get; set; } = new List<ProductVariantAttribute>();
}

public enum AttributeDataType
{
    Text = 0,
    Number = 1,
    Boolean = 2,
    Option = 3
}
