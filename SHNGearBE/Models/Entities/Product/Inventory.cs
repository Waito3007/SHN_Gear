using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Product;

public class Inventory : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public int Quantity { get; set; }
    public int SafetyStock { get; set; }
    public string? Location { get; set; }

    public virtual Product Product { get; set; } = null!;
}
