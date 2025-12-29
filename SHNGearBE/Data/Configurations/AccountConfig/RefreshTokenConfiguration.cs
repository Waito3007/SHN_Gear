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
        builder.Property(x => x.Token).IsRequired().HasMaxLength(256);
        builder.Property(x => x.JwtToken).IsRequired().HasMaxLength(256);
        builder.Property(x => x.isUsed).IsRequired().HasMaxLength(5);
        builder.Property(x => x.isRevoked).IsRequired().HasMaxLength(5);
        builder.Property(x => x.Expires).IsRequired().HasMaxLength(30);

        // Foreign Key
        builder.HasOne(x => x.Account)
        .WithMany(x => x.RefreshTokens)
        .HasForeignKey(x => x.AccountId)
        .OnDelete(DeleteBehavior.Cascade);

    }
}
