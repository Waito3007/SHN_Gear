
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Account;

namespace SHNGearBE.Data.Configuration.AccountConfig;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Primary
        builder.HasKey(x => x.Id);
        //Property
        builder.Property(x => x.Username).HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
        // Because Hash don't need Vietnamese,emoji so we use for estimate date(1bit IsUnicodeFalse)
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256).IsUnicode(false);
        builder.Property(x => x.Salt).IsRequired().HasMaxLength(256).IsUnicode(false);
    }
}