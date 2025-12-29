using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Account;

namespace SHNGearBE.Data.Configurations.AccountConfig;

public class AccountDetailConfiguration : IEntityTypeConfiguration<AccountDetail>
{
    public void Configure(EntityTypeBuilder<AccountDetail> builder)
    {
        // Primary key
        builder.HasKey(x => x.Id);
        // Property
        builder.Property(x => x.FirstName).HasMaxLength(50);
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.Property(x => x.PhoneNumber).HasMaxLength(12);
        builder.Property(x => x.Address).HasMaxLength(256);
        // Foreign Key
        builder.HasOne(x => x.Account)
        .WithOne(x => x.AccountDetail)
        .HasForeignKey<AccountDetail>(x => x.AccountId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}
