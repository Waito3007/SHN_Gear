using System;

namespace SHNGearBE.Models.Entities.Account;

public class AccountRole
{
    public Guid AccountId { get; set; }
    public Guid RoleId { get; set; }

    public virtual Account Account { get; set; }
    public virtual Role Role { get; set; }
}
