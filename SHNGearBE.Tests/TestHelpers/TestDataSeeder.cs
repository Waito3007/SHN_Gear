using SHNGearBE.Data;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Tests.TestHelpers;

public static class TestDataSeeder
{
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
