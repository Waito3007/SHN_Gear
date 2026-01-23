using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class Review : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }

    public virtual Product Product { get; set; } = null!;
}
