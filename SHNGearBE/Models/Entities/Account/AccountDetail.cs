using System;

namespace SHNGearBE.Models.Entities.Account;

public class AccountDetail : BaseEntity
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; }
}
