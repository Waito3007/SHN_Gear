using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Account;

namespace SHNGearBE.Data.Configurations.AccountConfig;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Key
        builder.HasKey(x => x.Id);

        //Property
        builder.Property(x => x.Token).IsRequired().HasMaxLength(512);
        builder.Property(x => x.JwtToken).IsRequired().HasMaxLength(2048); // JWT tokens can be long
        builder.Property(x => x.IsUsed).IsRequired();
        builder.Property(x => x.IsRevoked).IsRequired();
        builder.Property(x => x.Expires).IsRequired();

        // Foreign Key
        builder.HasOne(x => x.Account)
        .WithMany(x => x.RefreshTokens)
        .HasForeignKey(x => x.AccountId)
        .OnDelete(DeleteBehavior.Cascade);

    }
}
