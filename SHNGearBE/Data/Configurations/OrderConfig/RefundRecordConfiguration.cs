using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Order;

namespace SHNGearBE.Data.Configurations.OrderConfig;

public class RefundRecordConfiguration : IEntityTypeConfiguration<RefundRecord>
{
    public void Configure(EntityTypeBuilder<RefundRecord> builder)
    {
        builder.ToTable("RefundRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RefundTransactionId)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.CaptureTransactionId)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.RefundAmountUsd)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalCapturedAmountUsd)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.RefundReason)
            .HasMaxLength(300);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.RefundTransactionId)
            .IsUnique();

        builder.HasOne(x => x.Order)
            .WithMany(o => o.RefundRecords)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
