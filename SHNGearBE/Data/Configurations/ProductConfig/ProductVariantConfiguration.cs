using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Name)
            .HasMaxLength(200);

        builder.Property(v => v.Quantity)
            .IsRequired();

        builder.Property(v => v.ReservedStock)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(v => v.SafetyStock)
            .HasDefaultValue(0);

        builder.HasIndex(v => v.Sku).IsUnique();

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
