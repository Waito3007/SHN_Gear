using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Data.Configurations.ProductConfig;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.HasIndex(p => p.Code).IsUnique();
        builder.HasIndex(p => p.Slug).IsUnique();

        // Performance indexes for filtering and sorting
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.BrandId);
        builder.HasIndex(p => p.CreateAt);
        builder.HasIndex(p => new { p.IsDelete, p.CreateAt });

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
