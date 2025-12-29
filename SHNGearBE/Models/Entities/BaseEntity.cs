
namespace SHNGearBE.Models.Entities;

public abstract class BaseEntity
{
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAt { get; set; }
    public bool IsDelete { get; set; }
}