using FluentAssertions;
using Xunit;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Tests.TestHelpers;

namespace SHNGearBE.Tests.IntegrationTests.ProductTests;

public class ProductServiceSearchIntegrationTests : ProductIntegrationTestBase
{
    [Fact]
    public async Task SearchAsync_WithSearchTerm_ShouldMatchNameCodeAndSlug()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Dell", "dell");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Laptops", "laptops");

        // Seed products that match search term in different fields
        await TestDataSeeder.SeedProduct(DbContext, "LAPTOP001", "Gaming Laptop XPS", "gaming-laptop-xps", brand.Id, category.Id, "XPS-001");
        await TestDataSeeder.SeedProduct(DbContext, "XPS002", "Professional Workstation", "professional-workstation", brand.Id, category.Id, "XPS-002");
        await TestDataSeeder.SeedProduct(DbContext, "SERVER001", "Dell PowerEdge", "dell-xps-server", brand.Id, category.Id, "SERVER-001");
        await TestDataSeeder.SeedProduct(DbContext, "MONITOR001", "4K Monitor", "4k-monitor", brand.Id, category.Id, "MON-001"); // Should not match

        var request = new ProductFilterRequest
        {
            SearchTerm = "xps",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Items.Should().Contain(p => p.Code == "LAPTOP001"); // Matches in Name
        result.Items.Should().Contain(p => p.Code == "XPS002"); // Matches in Code
        result.Items.Should().Contain(p => p.Code == "SERVER001"); // Matches in Slug
    }

    [Fact]
    public async Task SearchAsync_WithCategoryId_ShouldReturnOnlyCategoryProducts()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Sony", "sony");
        var electronicsCategory = await TestDataSeeder.SeedCategory(DbContext, "Electronics", "electronics");
        var gamesCategory = await TestDataSeeder.SeedCategory(DbContext, "Games", "games");

        // Seed products in different categories
        await TestDataSeeder.SeedProduct(DbContext, "TV001", "Sony TV", "sony-tv", brand.Id, electronicsCategory.Id, "TV-55");
        await TestDataSeeder.SeedProduct(DbContext, "GAME001", "PlayStation 5", "playstation-5", brand.Id, gamesCategory.Id, "PS5");
        await TestDataSeeder.SeedProduct(DbContext, "PHONE001", "Xperia Phone", "xperia-phone", brand.Id, electronicsCategory.Id, "XPERIA");

        var request = new ProductFilterRequest
        {
            CategoryId = electronicsCategory.Id,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(p => p.CategoryName == "Electronics");
        result.Items.Should().NotContain(p => p.Code == "GAME001");
    }

    [Fact]
    public async Task SearchAsync_WithBrandId_ShouldReturnOnlyBrandProducts()
    {
        // Arrange
        await CleanupDatabase();

        var nikeBrand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var adidasBrand = await TestDataSeeder.SeedBrand(DbContext, "Adidas", "adidas");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Shoes", "shoes");

        // Seed products from different brands
        await TestDataSeeder.SeedProduct(DbContext, "SHOE001", "Nike Air Max", "nike-air-max", nikeBrand.Id, category.Id, "AIR-MAX");
        await TestDataSeeder.SeedProduct(DbContext, "SHOE002", "Nike Jordan", "nike-jordan", nikeBrand.Id, category.Id, "JORDAN");
        await TestDataSeeder.SeedProduct(DbContext, "SHOE003", "Adidas Superstar", "adidas-superstar", adidasBrand.Id, category.Id, "SUPERSTAR");

        var request = new ProductFilterRequest
        {
            BrandId = nikeBrand.Id,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(p => p.BrandName == "Nike");
        result.Items.Should().NotContain(p => p.Code == "SHOE003");
    }

    [Fact]
    public async Task SearchAsync_CombinedFilters_ShouldApplyAllConditions()
    {
        // Arrange
        await CleanupDatabase();

        var samsungBrand = await TestDataSeeder.SeedBrand(DbContext, "Samsung", "samsung");
        var lgBrand = await TestDataSeeder.SeedBrand(DbContext, "LG", "lg");
        var tvCategory = await TestDataSeeder.SeedCategory(DbContext, "Televisions", "televisions");
        var phoneCategory = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");

        // Seed various products
        await TestDataSeeder.SeedProduct(DbContext, "TV001", "Samsung QLED TV", "samsung-qled-tv", samsungBrand.Id, tvCategory.Id, "QLED-55");
        await TestDataSeeder.SeedProduct(DbContext, "PHONE001", "Samsung Galaxy S24", "samsung-galaxy-s24", samsungBrand.Id, phoneCategory.Id, "GALAXY");
        await TestDataSeeder.SeedProduct(DbContext, "TV002", "LG OLED TV", "lg-oled-tv", lgBrand.Id, tvCategory.Id, "OLED-55");

        var request = new ProductFilterRequest
        {
            SearchTerm = "samsung",
            CategoryId = tvCategory.Id,
            BrandId = samsungBrand.Id,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().Code.Should().Be("TV001");
        result.Items.First().BrandName.Should().Be("Samsung");
        result.Items.First().CategoryName.Should().Be("Televisions");
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectSubset()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Generic", "generic");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Products", "products");

        // Seed 10 products
        for (int i = 1; i <= 10; i++)
        {
            await TestDataSeeder.SeedProduct(
                DbContext,
                $"PROD{i:000}",
                $"Product {i}",
                $"product-{i}",
                brand.Id,
                category.Id,
                $"SKU-{i}"
            );
        }

        var requestPage1 = new ProductFilterRequest { Page = 1, PageSize = 3 };
        var requestPage2 = new ProductFilterRequest { Page = 2, PageSize = 3 };

        // Act
        var resultPage1 = await ProductService.SearchAsync(requestPage1);
        var resultPage2 = await ProductService.SearchAsync(requestPage2);

        // Assert
        resultPage1.Should().NotBeNull();
        resultPage1.Items.Should().HaveCount(3);
        resultPage1.TotalCount.Should().Be(10);
        resultPage1.TotalPages.Should().Be(4);
        resultPage1.HasPreviousPage.Should().BeFalse();
        resultPage1.HasNextPage.Should().BeTrue();

        resultPage2.Should().NotBeNull();
        resultPage2.Items.Should().HaveCount(3);
        resultPage2.TotalCount.Should().Be(10);
        resultPage2.HasPreviousPage.Should().BeTrue();
        resultPage2.HasNextPage.Should().BeTrue();

        // Pages should have different products
        var page1Codes = resultPage1.Items.Select(p => p.Code).ToList();
        var page2Codes = resultPage2.Items.Select(p => p.Code).ToList();
        page1Codes.Should().NotIntersectWith(page2Codes);
    }

    [Fact]
    public async Task SearchAsync_EmptyDatabase_ShouldReturnEmptyResult()
    {
        // Arrange
        await CleanupDatabase();

        var request = new ProductFilterRequest
        {
            SearchTerm = "anything",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_NoMatchingResults_ShouldReturnEmptyResult()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Apple", "apple");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");
        await TestDataSeeder.SeedProduct(DbContext, "PHONE001", "iPhone 15", "iphone-15", brand.Id, category.Id, "IPHONE");

        var request = new ProductFilterRequest
        {
            SearchTerm = "samsung",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ShouldExcludeSoftDeletedProducts()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Microsoft", "microsoft");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Laptops", "laptops");

        var product1 = await TestDataSeeder.SeedProduct(DbContext, "LAPTOP001", "Surface Pro", "surface-pro", brand.Id, category.Id, "SURFACE-PRO");
        var product2 = await TestDataSeeder.SeedProduct(DbContext, "LAPTOP002", "Surface Laptop", "surface-laptop", brand.Id, category.Id, "SURFACE-LAPTOP");

        // Soft delete one product
        await ProductService.DeleteAsync(product1.Id);

        var request = new ProductFilterRequest
        {
            SearchTerm = "surface",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().Code.Should().Be("LAPTOP002");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnLowestPriceFromVariants()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "HP", "hp");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Laptops", "laptops");
        var product = await TestDataSeeder.SeedProduct(DbContext, "LAPTOP001", "HP Pavilion", "hp-pavilion", brand.Id, category.Id, "HP-128GB");

        // Add a more expensive variant
        var variant256 = new SHNGearBE.Models.Entities.Product.ProductVariant
        {
            ProductId = product.Id,
            Sku = "HP-256GB",
            Name = "256GB",
            Quantity = 30,
            SafetyStock = 5
        };
        DbContext.ProductVariants.Add(variant256);

        var price256 = new SHNGearBE.Models.Entities.Product.ProductVariantPrice
        {
            ProductVariant = variant256,
            BasePrice = 1200m,
            SalePrice = 1100m,
            Currency = "USD",
            ValidFrom = DateTime.UtcNow
        };
        DbContext.ProductVariantPrices.Add(price256);

        await DbContext.SaveChangesAsync();

        var request = new ProductFilterRequest
        {
            SearchTerm = "pavilion",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await ProductService.SearchAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);

        // Should return the lowest price (from default variant seeded in TestDataSeeder)
        var productResult = result.Items.First();
        productResult.SalePrice.Should().BeLessThan(1100m);
    }
}
