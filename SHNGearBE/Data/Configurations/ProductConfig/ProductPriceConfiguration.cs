using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");
        builder.HasKey(pp => pp.Id);

        builder.Property(pp => pp.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("USD")
            .IsRequired();

        builder.Property(pp => pp.BasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pp => pp.SalePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(pp => pp.ValidFrom)
            .IsRequired();

        builder.HasOne(pp => pp.Product)
            .WithMany(p => p.Prices)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
