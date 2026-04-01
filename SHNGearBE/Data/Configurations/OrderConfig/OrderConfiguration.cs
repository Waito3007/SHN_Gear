using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Order;

namespace SHNGearBE.Data.Configurations.OrderConfig;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(o => o.Note)
            .HasMaxLength(500);

        builder.Property(o => o.CancelledReason)
            .HasMaxLength(300);

        builder.Property(o => o.SubTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.ShippingFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentProvider)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.PaymentTransactionId)
            .HasMaxLength(120);

        builder.HasIndex(o => o.Code)
            .IsUnique();

        builder.HasIndex(o => o.AccountId);

        builder.HasOne(o => o.Account)
            .WithMany()
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.DeliveryAddress)
            .WithMany()
            .HasForeignKey(o => o.DeliveryAddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
