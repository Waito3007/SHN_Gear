using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");
        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.Value)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.ProductAttributes)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.AttributeDefinition)
            .WithMany(ad => ad.ProductAttributes)
            .HasForeignKey(pa => pa.ProductAttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
