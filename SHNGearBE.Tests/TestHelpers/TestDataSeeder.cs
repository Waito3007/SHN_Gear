using SHNGearBE.Data;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Tests.TestHelpers;

public static class TestDataSeeder
{
    public static async Task<Brand> SeedBrand(ApplicationDbContext context, string name, string slug)
    {
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Brand: {name}",
            CreateAt = DateTime.UtcNow
        };

        context.Brands.Add(brand);
        await context.SaveChangesAsync();

        return brand;
    }

    public static async Task<Category> SeedCategory(ApplicationDbContext context, string name, string slug)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreateAt = DateTime.UtcNow
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return category;
    }

    public static async Task<Product> SeedProduct(
        ApplicationDbContext context,
        string code,
        string name,
        string slug,
        Guid brandId,
        Guid categoryId,
        string variantSku)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Slug = slug,
            Description = $"Description for {name}",
            CategoryId = categoryId,
            BrandId = brandId,
            CreateAt = DateTime.UtcNow,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = variantSku,
                    Name = "Default Variant",
                    Quantity = 100,
                    ReservedStock = 0,
                    SafetyStock = 10,
                    CreateAt = DateTime.UtcNow,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice
                        {
                            Id = Guid.NewGuid(),
                            BasePrice = 1000m,
                            SalePrice = 899m,
                            Currency = "USD",
                            ValidFrom = DateTime.UtcNow,
                            CreateAt = DateTime.UtcNow
                        }
                    }
                }
            }
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product;
    }

    public static (Brand brand, Category category) SeedReferenceData(ApplicationDbContext context)
    {
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = "Test Brand",
            Description = "Brand for testing"
        };

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Slug = "test-category"
        };

        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.SaveChanges();

        return (brand, category);
    }

    public static Product SeedProduct(ApplicationDbContext context, Guid brandId, Guid categoryId, string code = "TEST-PROD")
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = "Test Product",
            Slug = $"test-product-{code.ToLowerInvariant()}",
            Description = "Product for testing",
            CategoryId = categoryId,
            BrandId = brandId,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = $"SKU-{code}",
                    Name = "Default Variant",
                    Quantity = 10,
                    SafetyStock = 2,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice
                        {
                            Id = Guid.NewGuid(),
                            BasePrice = 100m,
                            SalePrice = 90m,
                            Currency = "USD",
                            ValidFrom = DateTime.UtcNow
                        }
                    }
                }
            }
        };

        context.Products.Add(product);
        context.SaveChanges();

        return product;
    }

    public static ProductAttributeDefinition SeedAttributeDefinition(ApplicationDbContext context, string name = "Size", AttributeDataType dataType = AttributeDataType.Text)
    {
        var attrDef = new ProductAttributeDefinition
        {
            Id = Guid.NewGuid(),
            Name = name,
            DataType = dataType
        };

        context.ProductAttributeDefinitions.Add(attrDef);
        context.SaveChanges();

        return attrDef;
    }
}
