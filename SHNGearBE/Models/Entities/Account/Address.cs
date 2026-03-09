using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Account;

public class Address : BaseEntity
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }

    public string RecipientName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? Note { get; set; }
    public bool IsDefault { get; set; }

    public virtual Account Account { get; set; } = null!;
}
