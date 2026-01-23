using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class ProductVariantAttribute : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductAttributeDefinitionId { get; set; }
    public string Value { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
    public virtual ProductAttributeDefinition AttributeDefinition { get; set; } = null!;
}
