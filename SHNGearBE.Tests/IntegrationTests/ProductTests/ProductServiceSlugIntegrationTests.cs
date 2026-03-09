using FluentAssertions;
using Xunit;
using SHNGearBE.Tests.TestHelpers;

namespace SHNGearBE.Tests.IntegrationTests.ProductTests;

public class ProductServiceSlugIntegrationTests : ProductIntegrationTestBase
{
    [Fact]
    public async Task GetBySlugAsync_ValidSlug_ShouldReturnProductWithFullDetails()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Shoes", "shoes");
        var product = await TestDataSeeder.SeedProduct(
            DbContext,
            "SHOE001",
            "Running Shoes",
            "running-shoes",
            brand.Id,
            category.Id,
            "SHOE-S"
        );

        // Act
        var result = await ProductService.GetBySlugAsync("running-shoes");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Code.Should().Be("SHOE001");
        result.Name.Should().Be("Running Shoes");
        result.Slug.Should().Be("running-shoes");
        result.BrandId.Should().Be(brand.Id);
        result.CategoryId.Should().Be(category.Id);
        result.Variants.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBySlugAsync_NonExistentSlug_ShouldReturnNull()
    {
        // Arrange
        await CleanupDatabase();

        // Act
        var result = await ProductService.GetBySlugAsync("non-existent-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_SoftDeletedProduct_ShouldReturnNull()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Adidas", "adidas");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");
        var product = await TestDataSeeder.SeedProduct(
            DbContext,
            "CLOTH001",
            "T-Shirt",
            "t-shirt",
            brand.Id,
            category.Id,
            "SHIRT-M"
        );

        // Soft delete product
        await ProductService.DeleteAsync(product.Id);

        // Act
        var result = await ProductService.GetBySlugAsync("t-shirt");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_CaseInsensitive_ShouldReturnProduct()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Samsung", "samsung");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Electronics", "electronics");
        await TestDataSeeder.SeedProduct(
            DbContext,
            "PHONE001",
            "Galaxy S24",
            "galaxy-s24",
            brand.Id,
            category.Id,
            "GALAXY-S24"
        );

        // Act - Query with different case
        var result = await ProductService.GetBySlugAsync("galaxy-s24");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Galaxy S24");
    }

    [Fact]
    public async Task GetBySlugAsync_WithMultipleVariants_ShouldReturnAllVariants()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Apple", "apple");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");
        var product = await TestDataSeeder.SeedProduct(
            DbContext,
            "IPHONE001",
            "iPhone 15",
            "iphone-15",
            brand.Id,
            category.Id,
            "IPHONE-128GB"
        );

        // Add more variants
        var variant256 = new SHNGearBE.Models.Entities.Product.ProductVariant
        {
            ProductId = product.Id,
            Sku = "IPHONE-256GB",
            Name = "256GB",
            Quantity = 50,
            SafetyStock = 10
        };
        DbContext.ProductVariants.Add(variant256);

        var price256 = new SHNGearBE.Models.Entities.Product.ProductVariantPrice
        {
            ProductVariant = variant256,
            BasePrice = 1099m,
            SalePrice = 999m,
            Currency = "USD",
            ValidFrom = DateTime.UtcNow
        };
        DbContext.ProductVariantPrices.Add(price256);

        await DbContext.SaveChangesAsync();

        // Act
        var result = await ProductService.GetBySlugAsync("iphone-15");

        // Assert
        result.Should().NotBeNull();
        result!.Variants.Should().HaveCount(2);
        result.Variants.Should().Contain(v => v.Sku == "IPHONE-128GB");
        result.Variants.Should().Contain(v => v.Sku == "IPHONE-256GB");
    }
}
