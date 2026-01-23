using Moq;
using Xunit;
using BackgroundLogService.Abstractions;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface;
using SHNGearBE.Services;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Tests.UnitTests.ProductTests;

public class ProductServiceGetPagedTests
{
    [Fact]
    public async Task GetPagedAsync_ValidParameters_ShouldReturnProducts()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var products = new List<Product>
        {
            CreateTestProduct("PROD-001", "Product 1", 100m),
            CreateTestProduct("PROD-002", "Product 2", 200m),
            CreateTestProduct("PROD-003", "Product 3", 150m)
        };

        mockRepo.Setup(r => r.GetPagedAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.GetPagedAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("PROD-001", result[0].Code);
        Assert.Equal(100m, result[0].BasePrice);
    }

    [Fact]
    public async Task GetPagedAsync_Page2_ShouldCalculateCorrectSkip()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.GetPagedAsync(20, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        await service.GetPagedAsync(3, 10, CancellationToken.None);

        // Assert
        mockRepo.Verify(r => r.GetPagedAsync(20, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_ZeroPage_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.GetPagedAsync(0, 10, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task GetPagedAsync_NegativePage_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.GetPagedAsync(-1, 10, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task GetPagedAsync_ZeroPageSize_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.GetPagedAsync(1, 0, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task GetPagedAsync_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.GetPagedAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPagedAsync_ProductsWithMultipleVariants_ShouldUseLowestPrice()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = "MULTI-VAR",
            Name = "Multi Variant Product",
            Slug = "multi-variant",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = "cat" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Brand" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = "VAR-1",
                    Quantity = 10,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice { BasePrice = 100m, SalePrice = 90m, Currency = "USD", ValidFrom = DateTime.UtcNow }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                },
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = "VAR-2",
                    Quantity = 5,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice { BasePrice = 150m, SalePrice = 120m, Currency = "USD", ValidFrom = DateTime.UtcNow }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                }
            },
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.GetPagedAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(100m, result[0].BasePrice);
        Assert.Equal(90m, result[0].SalePrice);
    }

    private static Product CreateTestProduct(string code, string name, decimal price)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Slug = name.ToLowerInvariant().Replace(" ", "-"),
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Test Category", Slug = "test-category" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = $"SKU-{code}",
                    Quantity = 10,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice
                        {
                            Id = Guid.NewGuid(),
                            BasePrice = price,
                            Currency = "USD",
                            ValidFrom = DateTime.UtcNow
                        }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                }
            },
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };
    }
}
