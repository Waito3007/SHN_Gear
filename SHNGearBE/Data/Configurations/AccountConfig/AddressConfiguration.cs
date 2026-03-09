using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Account;

namespace SHNGearBE.Data.Configurations.AccountConfig;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.RecipientName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Province)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.District)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Ward)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Street)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(a => a.Note)
            .HasMaxLength(500);

        builder.HasIndex(a => a.AccountId);
        builder.HasIndex(a => new { a.AccountId, a.IsDefault });

        builder.HasOne(a => a.Account)
            .WithMany(acc => acc.Addresses)
            .HasForeignKey(a => a.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
