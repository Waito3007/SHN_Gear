using System;

namespace SHNGearBE.Models.Entities.Account;

public class Role : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public virtual ICollection<AccountRole> AccountRoles { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; }
}
