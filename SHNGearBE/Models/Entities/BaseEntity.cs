
namespace SHNGearBE.Models.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAt { get; set; }
    public bool isDelete { get; set; }
}
