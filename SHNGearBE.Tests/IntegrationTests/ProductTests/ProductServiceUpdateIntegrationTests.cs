using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Tests.TestHelpers;
using Xunit;

namespace SHNGearBE.Tests.IntegrationTests.ProductTests;

public class ProductServiceUpdateIntegrationTests : ProductIntegrationTestBase
{
    [Fact]
    public async Task UpdateAsync_ValidRequest_ShouldPersistChangesToDatabase()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");
        var product = await TestDataSeeder.SeedProduct(DbContext, "PROD001", "Old Name", "old-name", brand.Id, category.Id, "SKU001");

        var request = new UpdateProductRequest
        {
            Id = product.Id,
            Code = "PROD001",
            Name = "Updated Name",
            Slug = "updated-name",
            Description = "Updated Description",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Id = product.Variants.First().Id,
                    Sku = "SKU001",
                    Name = "Updated Variant",
                    Quantity = 200,
                    SafetyStock = 20,
                    BasePrice = 150m,
                    SalePrice = 120m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.UpdateAsync(product.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify changes in database
        var productInDb = await DbContext.Products
            .Include(p => p.Variants)
                .ThenInclude(v => v.Prices.OrderByDescending(p => p.ValidFrom))
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.Name.Should().Be("Updated Name");
        productInDb.Slug.Should().Be("updated-name");
        productInDb.Description.Should().Be("Updated Description");

        var variant = productInDb.Variants.First();
        variant.Name.Should().Be("Updated Variant");
        variant.Quantity.Should().Be(200);
        variant.SafetyStock.Should().Be(20);

        // Check price history - should have 2 price records (old closed, new active)
        variant.Prices.Should().HaveCountGreaterOrEqualTo(1);
        var currentPrice = variant.Prices.First(p => p.ValidTo == null);
        currentPrice.BasePrice.Should().Be(150m);
        currentPrice.SalePrice.Should().Be(120m);
    }

    [Fact]
    public async Task UpdateAsync_AddNewVariant_ShouldPersistAdditionalVariant()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Adidas", "adidas");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Shoes", "shoes");
        var product = await TestDataSeeder.SeedProduct(DbContext, "SHOE001", "Running Shoes", "running-shoes", brand.Id, category.Id, "SHOE-S");

        var request = new UpdateProductRequest
        {
            Id = product.Id,
            Code = "SHOE001",
            Name = "Running Shoes",
            Slug = "running-shoes",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                // Existing variant
                new ProductVariantRequest
                {
                    Id = product.Variants.First().Id,
                    Sku = "SHOE-S",
                    Quantity = 50,
                    SafetyStock = 5,
                    BasePrice = 100m,
                    Currency = "USD"
                },
                // New variant
                new ProductVariantRequest
                {
                    Sku = "SHOE-M",
                    Name = "Size M",
                    Quantity = 75,
                    SafetyStock = 10,
                    BasePrice = 100m,
                    SalePrice = 85m,
                    Currency = "USD",
                    Attributes = new Dictionary<string, string> { { "Size", "M" } }
                }
            }
        };

        // Act
        var result = await ProductService.UpdateAsync(product.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify both variants in database
        var productInDb = await DbContext.Products
            .Include(p => p.Variants)
                .ThenInclude(v => v.Prices)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.Variants.Should().HaveCount(2);

        var sizeS = productInDb.Variants.FirstOrDefault(v => v.Sku == "SHOE-S");
        sizeS.Should().NotBeNull();

        var sizeM = productInDb.Variants.FirstOrDefault(v => v.Sku == "SHOE-M");
        sizeM.Should().NotBeNull();
        sizeM!.Name.Should().Be("Size M");
        sizeM.Quantity.Should().Be(75);
        sizeM.Prices.First().SalePrice.Should().Be(85m);
        sizeM.Attributes.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_PriceChange_ShouldCreatePriceHistory()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Samsung", "samsung");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Electronics", "electronics");
        var product = await TestDataSeeder.SeedProduct(DbContext, "ELEC001", "Phone", "phone", brand.Id, category.Id, "PHONE-001");

        var variantId = product.Variants.First().Id;
        var oldPriceCount = await DbContext.ProductVariantPrices.CountAsync(p => p.ProductVariantId == variantId);

        var request = new UpdateProductRequest
        {
            Id = product.Id,
            Code = "ELEC001",
            Name = "Phone",
            Slug = "phone",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Id = variantId,
                    Sku = "PHONE-001",
                    Quantity = 100,
                    SafetyStock = 10,
                    BasePrice = 899m, // Changed from 100m
                    SalePrice = 799m,  // New sale price
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.UpdateAsync(product.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify price history
        var pricesInDb = await DbContext.ProductVariantPrices
            .Where(p => p.ProductVariantId == variantId)
            .OrderBy(p => p.ValidFrom)
            .ToListAsync();

        pricesInDb.Should().HaveCount(oldPriceCount + 1);

        // Old price should be closed (ValidTo set)
        var oldPrice = pricesInDb.First();
        oldPrice.ValidTo.Should().NotBeNull();
        oldPrice.ValidTo.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // New price should be active (ValidTo null)
        var newPrice = pricesInDb.Last();
        newPrice.BasePrice.Should().Be(899m);
        newPrice.SalePrice.Should().Be(799m);
        newPrice.ValidFrom.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        newPrice.ValidTo.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_RemoveVariant_ShouldDeleteFromDatabase()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Apple", "apple");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");

        // Create product with 2 variants
        var product = await TestDataSeeder.SeedProduct(DbContext, "IPHONE001", "iPhone", "iphone", brand.Id, category.Id, "IPHONE-128");

        var variant2 = new SHNGearBE.Models.Entities.Product.ProductVariant
        {
            ProductId = product.Id,
            Sku = "IPHONE-256",
            Name = "256GB",
            Quantity = 50,
            SafetyStock = 5
        };
        DbContext.ProductVariants.Add(variant2);

        var price2 = new SHNGearBE.Models.Entities.Product.ProductVariantPrice
        {
            ProductVariant = variant2,
            BasePrice = 1099m,
            Currency = "USD",
            ValidFrom = DateTime.UtcNow
        };
        DbContext.ProductVariantPrices.Add(price2);
        await DbContext.SaveChangesAsync();

        var variant2Id = variant2.Id;

        var request = new UpdateProductRequest
        {
            Id = product.Id,
            Code = "IPHONE001",
            Name = "iPhone",
            Slug = "iphone",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                // Only keep first variant
                new ProductVariantRequest
                {
                    Id = product.Variants.First().Id,
                    Sku = "IPHONE-128",
                    Quantity = 100,
                    SafetyStock = 10,
                    BasePrice = 999m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.UpdateAsync(product.Id, request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify only one variant remains
        var productInDb = await DbContext.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.Variants.Should().HaveCount(1);
        productInDb.Variants.First().Sku.Should().Be("IPHONE-128");

        // Verify second variant is deleted
        var deletedVariant = await DbContext.ProductVariants.FindAsync(variant2Id);
        deletedVariant.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCode_ShouldFailValidation()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");

        var product1 = await TestDataSeeder.SeedProduct(DbContext, "PROD001", "Product 1", "product-1", brand.Id, category.Id, "SKU001");
        var product2 = await TestDataSeeder.SeedProduct(DbContext, "PROD002", "Product 2", "product-2", brand.Id, category.Id, "SKU002");

        var request = new UpdateProductRequest
        {
            Id = product2.Id,
            Code = "PROD001", // Duplicate code from product1
            Name = "Product 2",
            Slug = "product-2",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Id = product2.Variants.First().Id,
                    Sku = "SKU002",
                    Quantity = 50,
                    SafetyStock = 5,
                    BasePrice = 50m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.UpdateAsync(product2.Id, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResponseType.Should().Be(ResponseType.AlreadyExists);

        // Verify product2 code unchanged in database
        var product2InDb = await DbContext.Products.FindAsync(product2.Id);
        product2InDb!.Code.Should().Be("PROD002");
    }
}
