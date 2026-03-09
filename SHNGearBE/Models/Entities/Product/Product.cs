using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class Product : BaseEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }

    public Guid CategoryId { get; set; }
    public Guid BrandId { get; set; }
    public bool IsFeatured { get; set; } = false;
    public int SoldCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;

    public virtual Category Category { get; set; } = null!;
    public virtual Brand Brand { get; set; } = null!;
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
