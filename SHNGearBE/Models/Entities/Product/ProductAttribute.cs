using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class ProductAttribute : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductAttributeDefinitionId { get; set; }
    public string Value { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
    public virtual ProductAttributeDefinition AttributeDefinition { get; set; } = null!;
}
