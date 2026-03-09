using Moq;
using Xunit;
using BackgroundLogService.Abstractions;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Services;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Tests.UnitTests.ProductTests;

public class ProductServiceGetBySlugTests
{
    [Fact]
    public async Task GetBySlugAsync_ValidSlug_ShouldReturnProduct()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId, "PROD-001", "Test Product", "test-product");

        mockRepo.Setup(r => r.GetBySlugCachedAsync("test-product", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.GetBySlugAsync("test-product", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PROD-001", result.Code);
        Assert.Equal("test-product", result.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_SlugNotFound_ShouldReturnNull()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.GetBySlugCachedAsync("non-existent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.GetBySlugAsync("non-existent", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySlugAsync_EmptySlug_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() =>
            service.GetBySlugAsync("", CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task GetBySlugAsync_NullSlug_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() =>
            service.GetBySlugAsync(null!, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task GetBySlugAsync_WhitespaceSlug_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() =>
            service.GetBySlugAsync("   ", CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    private static Product CreateTestProduct(Guid id, string code, string name, string slug)
    {
        return new Product
        {
            Id = id,
            Code = code,
            Name = name,
            Slug = slug,
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Test Category", Slug = "test-category" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = "SKU-001",
                    Quantity = 10,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice
                        {
                            BasePrice = 100m,
                            SalePrice = 90m,
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
