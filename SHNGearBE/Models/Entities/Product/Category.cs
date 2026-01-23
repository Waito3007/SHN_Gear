using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class Category : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }

    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
