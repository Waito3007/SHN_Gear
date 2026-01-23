using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductAttributeDefinitionConfiguration : IEntityTypeConfiguration<ProductAttributeDefinition>
{
    public void Configure(EntityTypeBuilder<ProductAttributeDefinition> builder)
    {
        builder.ToTable("ProductAttributeDefinitions");
        builder.HasKey(pad => pad.Id);

        builder.Property(pad => pad.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(pad => pad.Name).IsUnique();

        builder.Property(pad => pad.DataType)
            .IsRequired();
    }
}
