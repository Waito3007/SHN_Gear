using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class ProductImage : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
