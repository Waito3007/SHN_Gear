using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class Brand : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
