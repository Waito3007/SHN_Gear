using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Tests.TestHelpers;
using Xunit;

namespace SHNGearBE.Tests.IntegrationTests.ProductTests;

public class ProductServiceCreateIntegrationTests : ProductIntegrationTestBase
{
    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldPersistToDatabase()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");

        var request = new CreateProductRequest
        {
            Code = "PROD001",
            Name = "Test Product",
            Slug = "test-product",
            Description = "Test Description",
            ShortDescription = "Test Short Desc",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Images = new List<string> { "image1.jpg" },
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "SKU001",
                    Name = "Variant 1",
                    Quantity = 100,
                    SafetyStock = 10,
                    BasePrice = 100m,
                    SalePrice = 90m,
                    Currency = "USD",
                    Attributes = new Dictionary<string, string>
                    {
                        { "Size", "M" },
                        { "Color", "Blue" }
                    }
                }
            }
        };

        // Act
        var result = await ProductService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        // Verify in database
        var productInDb = await DbContext.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Prices)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
                    .ThenInclude(a => a.AttributeDefinition)
            .FirstOrDefaultAsync(p => p.Code == "PROD001");

        productInDb.Should().NotBeNull();
        productInDb!.Name.Should().Be("Test Product");
        productInDb.Slug.Should().Be("test-product");
        productInDb.BrandId.Should().Be(brand.Id);
        productInDb.CategoryId.Should().Be(category.Id);
        productInDb.Variants.Should().HaveCount(1);

        var variant = productInDb.Variants.First();
        variant.Sku.Should().Be("SKU001");
        variant.Quantity.Should().Be(100);
        variant.SafetyStock.Should().Be(10);
        variant.Prices.Should().HaveCount(1);

        var price = variant.Prices.First();
        price.BasePrice.Should().Be(100m);
        price.SalePrice.Should().Be(90m);
        price.Currency.Should().Be("USD");
        price.ValidFrom.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        price.ValidTo.Should().BeNull();

        variant.Attributes.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldFailWithDatabaseConstraint()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");

        // Create first product
        await TestDataSeeder.SeedProduct(DbContext, "PROD001", "Product 1", "product-1", brand.Id, category.Id, "SKU001");

        var request = new CreateProductRequest
        {
            Code = "PROD001", // Duplicate code
            Name = "Product 2",
            Slug = "product-2",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "SKU002",
                    Quantity = 50,
                    SafetyStock = 5,
                    BasePrice = 50m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ResponseType.Should().Be(ResponseType.AlreadyExists);
        result.Message.Should().Contain("Product code or slug already exists");

        // Verify only one product exists in database
        var productCount = await DbContext.Products.CountAsync(p => p.Code == "PROD001");
        productCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSku_ShouldFailWithDatabaseConstraint()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Nike", "nike");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Clothing", "clothing");

        // Create first product with SKU001
        await TestDataSeeder.SeedProduct(DbContext, "PROD001", "Product 1", "product-1", brand.Id, category.Id, "SKU001");

        var request = new CreateProductRequest
        {
            Code = "PROD002",
            Name = "Product 2",
            Slug = "product-2",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "SKU001", // Duplicate SKU across different products
                    Quantity = 50,
                    SafetyStock = 5,
                    BasePrice = 50m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ResponseType.Should().Be(ResponseType.AlreadyExists);
        result.Message.Should().Contain("SKU already exists");

        // Verify second product was not created
        var productExists = await DbContext.Products.AnyAsync(p => p.Code == "PROD002");
        productExists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_MultipleVariants_ShouldPersistAllVariants()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Adidas", "adidas");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Shoes", "shoes");

        var request = new CreateProductRequest
        {
            Code = "SHOE001",
            Name = "Running Shoes",
            Slug = "running-shoes",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "SHOE-S",
                    Name = "Size S",
                    Quantity = 50,
                    SafetyStock = 5,
                    BasePrice = 100m,
                    SalePrice = 85m,
                    Currency = "USD",
                    Attributes = new Dictionary<string, string> { { "Size", "S" } }
                },
                new ProductVariantRequest
                {
                    Sku = "SHOE-M",
                    Name = "Size M",
                    Quantity = 75,
                    SafetyStock = 10,
                    BasePrice = 100m,
                    SalePrice = 80m,
                    Currency = "USD",
                    Attributes = new Dictionary<string, string> { { "Size", "M" } }
                },
                new ProductVariantRequest
                {
                    Sku = "SHOE-L",
                    Name = "Size L",
                    Quantity = 60,
                    SafetyStock = 8,
                    BasePrice = 100m,
                    SalePrice = 90m,
                    Currency = "USD",
                    Attributes = new Dictionary<string, string> { { "Size", "L" } }
                }
            }
        };

        // Act
        var result = await ProductService.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify all variants in database
        var productInDb = await DbContext.Products
            .Include(p => p.Variants)
                .ThenInclude(v => v.Prices)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
            .FirstOrDefaultAsync(p => p.Code == "SHOE001");

        productInDb.Should().NotBeNull();
        productInDb!.Variants.Should().HaveCount(3);

        var sizeS = productInDb.Variants.First(v => v.Sku == "SHOE-S");
        sizeS.Quantity.Should().Be(50);
        sizeS.Prices.First().SalePrice.Should().Be(85m);

        var sizeM = productInDb.Variants.First(v => v.Sku == "SHOE-M");
        sizeM.Quantity.Should().Be(75);
        sizeM.Prices.First().SalePrice.Should().Be(80m);

        var sizeL = productInDb.Variants.First(v => v.Sku == "SHOE-L");
        sizeL.Quantity.Should().Be(60);
        sizeL.Prices.First().SalePrice.Should().Be(90m);
    }

    [Fact]
    public async Task CreateAsync_WithTags_ShouldCreateProductTagRelationships()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Samsung", "samsung");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Electronics", "electronics");

        var request = new CreateProductRequest
        {
            Code = "ELEC001",
            Name = "Smartphone",
            Slug = "smartphone",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Tags = new List<string> { "New", "Trending", "5G" },
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "PHONE-128GB",
                    Quantity = 100,
                    SafetyStock = 20,
                    BasePrice = 799m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify tags in database
        var productInDb = await DbContext.Products
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Code == "ELEC001");

        productInDb.Should().NotBeNull();
        productInDb!.ProductTags.Should().HaveCount(3);

        var tagNames = productInDb.ProductTags.Select(pt => pt.Tag.Name).ToList();
        tagNames.Should().Contain("New");
        tagNames.Should().Contain("Trending");
        tagNames.Should().Contain("5G");

        // Verify tags are created in Tags table
        var tagsInDb = await DbContext.Tags.CountAsync();
        tagsInDb.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task CreateAsync_TransactionRollback_ShouldNotPersistOnFailure()
    {
        // Arrange
        await CleanupDatabase();

        var brand = await TestDataSeeder.SeedBrand(DbContext, "Apple", "apple");
        var category = await TestDataSeeder.SeedCategory(DbContext, "Phones", "phones");

        var request = new CreateProductRequest
        {
            Code = "PHONE001",
            Name = "iPhone",
            Slug = "iphone",
            BrandId = brand.Id,
            CategoryId = category.Id,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "IPHONE-001",
                    Quantity = -10, // Invalid: negative quantity
                    SafetyStock = 5,
                    BasePrice = 999m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await ProductService.CreateAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ResponseType.Should().Be(ResponseType.InvalidData);

        // Verify nothing was persisted to database
        var productExists = await DbContext.Products.AnyAsync(p => p.Code == "PHONE001");
        productExists.Should().BeFalse();

        var variantExists = await DbContext.ProductVariants.AnyAsync(v => v.Sku == "IPHONE-001");
        variantExists.Should().BeFalse();
    }
}
