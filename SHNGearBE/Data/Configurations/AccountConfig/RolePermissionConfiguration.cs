using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Account;

namespace SHNGearBE.Data.Configurations.AccountConfig;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder.HasOne(x => x.Role)
        .WithMany(x => x.RolePermissions)
        .HasForeignKey(x => x.RoleId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
       .WithMany(x => x.RolePermisions)
       .HasForeignKey(x => x.PermissionId)
       .OnDelete(DeleteBehavior.Cascade);
    }


}
