using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Tests.TestHelpers;
using Xunit;

namespace SHNGearBE.Tests.IntegrationTests.ProductTests;

public class ProductServiceGetIntegrationTests : ProductIntegrationTestBase
{
    [Fact]
    public async Task GetByIdAsync_ValidId_ShouldReturnProductWithAllRelationships()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");
        var product = await TestDataSeeder.SeedProduct(DbContext, "PROD001", "Product 1", "product-1", brand.Id, category.Id, "SKU001");

        // Act
        var result = await ProductService.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();

        result!.Id.Should().Be(product.Id);
        result.Code.Should().Be("PROD001");
        result.Name.Should().Be("Product 1");
        result.Slug.Should().Be("product-1");
        result.BrandName.Should().Be("Nike");
        result.CategoryName.Should().Be("Clothing");
        result.Variants.Should().HaveCount(1);

        var variant = result.Variants.First();
        variant.Sku.Should().Be("SKU001");
        variant.BasePrice.Should().Be(1000m);
        variant.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetByIdAsync_ProductWithMultipleVariants_ShouldReturnAllVariants()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Adidas", "adidas");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Shoes", "shoes");

        var product = await TestDataSeeder.SeedProduct(DbContext, "SHOE001", "Running Shoes", "running-shoes", brand.Id, category.Id, "SHOE-S");

        // Add more variants
        var variantM = new SHNGearBE.Models.Entities.Product.ProductVariant
        {
            ProductId = product.Id,
            Sku = "SHOE-M",
            Name = "Size M",
            Quantity = 75,
            SafetyStock = 10
        };
        DbContext.ProductVariants.Add(variantM);

        var priceM = new SHNGearBE.Models.Entities.Product.ProductVariantPrice
        {
            ProductVariant = variantM,
            BasePrice = 100m,
            SalePrice = 85m,
            Currency = "USD",
            ValidFrom = DateTime.UtcNow
        };
        DbContext.ProductVariantPrices.Add(priceM);

        var variantL = new SHNGearBE.Models.Entities.Product.ProductVariant
        {
            ProductId = product.Id,
            Sku = "SHOE-L",
            Name = "Size L",
            Quantity = 60,
            SafetyStock = 8
        };
        DbContext.ProductVariants.Add(variantL);

        var priceL = new SHNGearBE.Models.Entities.Product.ProductVariantPrice
        {
            ProductVariant = variantL,
            BasePrice = 100m,
            SalePrice = 90m,
            Currency = "USD",
            ValidFrom = DateTime.UtcNow
        };
        DbContext.ProductVariantPrices.Add(priceL);

        await DbContext.SaveChangesAsync();

        // Act
        var result = await ProductService.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Variants.Should().HaveCount(3);

        var skus = result.Variants.Select(v => v.Sku).ToList();
        skus.Should().Contain("SHOE-S");
        skus.Should().Contain("SHOE-M");
        skus.Should().Contain("SHOE-L");

        var sizeM = result.Variants.First(v => v.Sku == "SHOE-M");
        sizeM.Quantity.Should().Be(75);
        sizeM.SalePrice.Should().Be(85m);

        var sizeL = result.Variants.First(v => v.Sku == "SHOE-L");
        sizeL.Quantity.Should().Be(60);
        sizeL.SalePrice.Should().Be(90m);
    }

    [Fact]
    public async Task GetPagedAsync_ValidParameters_ShouldReturnCorrectPage()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Samsung", "samsung");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Electronics", "electronics");

        // Create 5 products
        for (int i = 1; i <= 5; i++)
        {
            await TestDataSeeder.SeedProduct(
                DbContext,
                $"PROD{i:D3}",
                $"Product {i}",
                $"product-{i}",
                brand.Id,
                category.Id,
                $"SKU{i:D3}"
            );
        }

        // Act
        var result = await ProductService.GetPagedAsync(1, 3);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(3);

        // Verify data
        var productCodes = result.Items.Select(p => p.Code).ToList();
        productCodes.Should().HaveCount(3);
        productCodes.All(c => c.StartsWith("PROD")).Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_ShouldSkipFirstPage()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Apple", "apple");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");

        // Create 10 products
        for (int i = 1; i <= 10; i++)
        {
            await TestDataSeeder.SeedProduct(
                DbContext,
                $"PHONE{i:D3}",
                $"iPhone {i}",
                $"iphone-{i}",
                brand.Id,
                category.Id,
                $"IPHONE-{i:D3}"
            );
        }

        // Act - Get page 2 with pageSize 3
        var result = await ProductService.GetPagedAsync(2, 3);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(3);

        // Page 2 should not contain items from page 1
        var codesPage1 = (await ProductService.GetPagedAsync(1, 3)).Items.Select(p => p.Code).ToList();
        var codesPage2 = result.Items.Select(p => p.Code).ToList();

        codesPage2.Should().NotIntersectWith(codesPage1);
    }

    [Fact]
    public async Task GetPagedAsync_ProductsWithMultiplePrices_ShouldReturnLowestPrice()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");

        var product = await TestDataSeeder.SeedProduct(DbContext, "CLOTH001", "T-Shirt", "t-shirt", brand.Id, category.Id, "SHIRT-S");

        // Add variant with lower price
        var variantM = new SHNGearBE.Models.Entities.Product.ProductVariant
        {
            ProductId = product.Id,
            Sku = "SHIRT-M",
            Name = "Size M",
            Quantity = 75,
            SafetyStock = 10
        };
        DbContext.ProductVariants.Add(variantM);

        var priceM = new SHNGearBE.Models.Entities.Product.ProductVariantPrice
        {
            ProductVariant = variantM,
            BasePrice = 80m,  // Lower base price
            SalePrice = 60m,  // Lower sale price
            Currency = "USD",
            ValidFrom = DateTime.UtcNow
        };
        DbContext.ProductVariantPrices.Add(priceM);

        await DbContext.SaveChangesAsync();

        // Act
        var result = await ProductService.GetPagedAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(1);

        var productItem = result.Items.First();
        // Should show lowest sale price from all variants (60m from Size M)
        productItem.SalePrice.Should().Be(60m);
    }

    [Fact]
    public async Task GetPagedAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        await CleanupDatabase();

        // Act
        var result = await ProductService.GetPagedAsync(1, 10);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_ExcludesSoftDeletedProducts()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "LG", "lg");
        var category = await TestDataSeeder.SeedCategory(DbContext, "TVs", "tvs");

        // Create 3 products
        var product1 = await TestDataSeeder.SeedProduct(DbContext, "TV001", "TV 1", "tv-1", brand.Id, category.Id, "TV-001");
        var product2 = await TestDataSeeder.SeedProduct(DbContext, "TV002", "TV 2", "tv-2", brand.Id, category.Id, "TV-002");
        var product3 = await TestDataSeeder.SeedProduct(DbContext, "TV003", "TV 3", "tv-3", brand.Id, category.Id, "TV-003");

        // Soft delete product2
        await ProductService.DeleteAsync(product2.Id);

        // Act
        var result = await ProductService.GetPagedAsync(1, 10);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);

        var codes = result.Items.Select(p => p.Code).ToList();
        codes.Should().Contain("TV001");
        codes.Should().Contain("TV003");
        codes.Should().NotContain("TV002"); // Soft deleted
    }
}
