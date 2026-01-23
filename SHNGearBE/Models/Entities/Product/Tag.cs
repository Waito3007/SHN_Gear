using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class Tag : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
