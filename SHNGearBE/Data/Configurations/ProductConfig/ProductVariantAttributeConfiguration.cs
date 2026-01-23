using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttribute>
{
    public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
    {
        builder.ToTable("ProductVariantAttributes");
        builder.HasKey(pva => pva.Id);

        builder.Property(pva => pva.Value)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(pva => pva.ProductVariant)
            .WithMany(v => v.VariantAttributes)
            .HasForeignKey(pva => pva.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pva => pva.AttributeDefinition)
            .WithMany(ad => ad.ProductVariantAttributes)
            .HasForeignKey(pva => pva.ProductAttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
