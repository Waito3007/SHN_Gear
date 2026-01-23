using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Tests.TestHelpers;
using Xunit;

namespace SHNGearBE.Tests.IntegrationTests.ProductTests;

public class ProductServiceDeleteIntegrationTests : ProductIntegrationTestBase
{
    [Fact]
    public async Task DeleteAsync_ValidId_ShouldSoftDeleteInDatabase()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");
        var product = await TestDataSeeder.SeedProduct(DbContext, "PROD001", "Product 1", "product-1", brand.Id, category.Id, "SKU001");

        // Act
        var result = await ProductService.DeleteAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify soft delete in database
        var productInDb = await DbContext.Products
            .IgnoreQueryFilters() // Include soft-deleted entities
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.IsDelete.Should().BeTrue();

        // Verify product is not returned in normal queries (filtered by IsDelete)
        var productNormalQuery = await DbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productNormalQuery.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeleted_ShouldBeIdempotent()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Adidas", "adidas");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Shoes", "shoes");
        var product = await TestDataSeeder.SeedProduct(DbContext, "SHOE001", "Shoes", "shoes", brand.Id, category.Id, "SHOE-001");

        // Delete first time
        await ProductService.DeleteAsync(product.Id);

        // Act - Delete second time
        var result = await ProductService.DeleteAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResponseType.Should().Be(ResponseType.NotFound);

        // Verify still soft deleted in database
        var productInDb = await DbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.IsDelete.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await CleanupDatabase();

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await ProductService.DeleteAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResponseType.Should().Be(ResponseType.NotFound);
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotDeleteVariantsPhysically()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Samsung", "samsung");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Electronics", "electronics");
        var product = await TestDataSeeder.SeedProduct(DbContext, "ELEC001", "Phone", "phone", brand.Id, category.Id, "PHONE-001");

        var variantId = product.Variants.First().Id;

        // Act
        var result = await ProductService.DeleteAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product is soft deleted
        var productInDb = await DbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.IsDelete.Should().BeTrue();

        // Verify variant still exists physically in database
        // (Note: Variants don't have soft delete, they remain for historical data)
        var variantInDb = await DbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId);

        variantInDb.Should().NotBeNull();
        variantInDb!.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task DeleteAsync_WithMultipleVariants_ShouldSoftDeleteProduct()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Apple", "apple");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");

        // Create product with multiple variants
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

        var variant1Id = product.Variants.First().Id;
        var variant2Id = variant2.Id;

        // Act
        var result = await ProductService.DeleteAsync(product.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product is soft deleted
        var productInDb = await DbContext.Products
            .IgnoreQueryFilters()
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        productInDb.Should().NotBeNull();
        productInDb!.IsDelete.Should().BeTrue();

        // Verify both variants still exist in database
        var variant1Exists = await DbContext.ProductVariants.AnyAsync(v => v.Id == variant1Id);
        var variant2Exists = await DbContext.ProductVariants.AnyAsync(v => v.Id == variant2Id);

        variant1Exists.Should().BeTrue();
        variant2Exists.Should().BeTrue();
    }
}
