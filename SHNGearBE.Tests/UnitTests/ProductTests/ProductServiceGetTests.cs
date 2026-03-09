using Moq;
using Xunit;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Services;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Tests.UnitTests.ProductTests;

public class ProductServiceGetTests
{
    [Fact]
    public async Task GetByIdAsync_ValidId_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = productId,
            Code = "PROD-001",
            Name = "Test Product",
            Slug = "test-product",
            Description = "A test product",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Samsung" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = variantId,
                    Sku = "SKU-001",
                    Name = "64GB Black",
                    Quantity = 100,
                    SafetyStock = 10,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice
                        {
                            Id = Guid.NewGuid(),
                            BasePrice = 999m,
                            SalePrice = 899m,
                            Currency = "USD",
                            ValidFrom = DateTime.UtcNow
                        }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                }
            },
            Images = new List<ProductImage>
            {
                new ProductImage { Id = Guid.NewGuid(), Url = "https://img.com/1.png", IsPrimary = true, SortOrder = 0 }
            },
            ProductTags = new List<ProductTag>
            {
                new ProductTag { Tag = new Tag { Id = Guid.NewGuid(), Name = "New" } }
            },
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetByIdWithDetailsCachedAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

        // Act
        var result = await service.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PROD-001", result.Code);
        Assert.Equal("Test Product", result.Name);
        Assert.Single(result.Variants);
        Assert.Equal("SKU-001", result.Variants[0].Sku);
        Assert.Equal(999m, result.Variants[0].BasePrice);
        Assert.Equal(899m, result.Variants[0].SalePrice);
        Assert.Single(result.ImageUrls);
        Assert.Single(result.Tags);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();

        mockRepo.Setup(r => r.GetByIdWithDetailsCachedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ProductWithMultipleVariants_ShouldReturnAllVariants()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = productId,
            Code = "SHIRT-001",
            Name = "T-Shirt",
            Slug = "t-shirt",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Clothing", Slug = "clothing" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Nike" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = "SHIRT-S",
                    Name = "Size S",
                    Quantity = 20,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice { BasePrice = 29.99m, Currency = "USD", ValidFrom = DateTime.UtcNow }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                },
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = "SHIRT-M",
                    Name = "Size M",
                    Quantity = 30,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice { BasePrice = 29.99m, Currency = "USD", ValidFrom = DateTime.UtcNow }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                },
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = "SHIRT-L",
                    Name = "Size L",
                    Quantity = 25,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice { BasePrice = 29.99m, Currency = "USD", ValidFrom = DateTime.UtcNow }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                }
            },
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetByIdWithDetailsCachedAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

        // Act
        var result = await service.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Variants.Count);
        Assert.Contains(result.Variants, v => v.Sku == "SHIRT-S");
        Assert.Contains(result.Variants, v => v.Sku == "SHIRT-M");
        Assert.Contains(result.Variants, v => v.Sku == "SHIRT-L");
    }
}

