using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductVariantPriceConfiguration : IEntityTypeConfiguration<ProductVariantPrice>
{
    public void Configure(EntityTypeBuilder<ProductVariantPrice> builder)
    {
        builder.ToTable("ProductVariantPrices");
        builder.HasKey(pvp => pvp.Id);

        builder.Property(pvp => pvp.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("USD")
            .IsRequired();

        builder.Property(pvp => pvp.BasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pvp => pvp.SalePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(pvp => pvp.ValidFrom)
            .IsRequired();

        builder.HasOne(pvp => pvp.ProductVariant)
            .WithMany(v => v.Prices)
            .HasForeignKey(pvp => pvp.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
