using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Account;

namespace SHNGearBE.Data.Configurations.AccountConfig;

public class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRole>
{
    public void Configure(EntityTypeBuilder<AccountRole> builder)
    {
        builder.HasKey(x => new { x.AccountId, x.RoleId });

        builder.HasOne(x => x.Account)
        .WithMany(x => x.AccountRoles)
        .HasForeignKey(x => x.AccountId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
        .WithMany(x => x.AccountRole)
        .HasForeignKey(x => x.RoleId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
