using Moq;
using Xunit;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Product;
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

        var products = new List<Product>
        {
            CreateTestProduct("PROD-001", "Product 1", 100m),
            CreateTestProduct("PROD-002", "Product 2", 200m),
            CreateTestProduct("PROD-003", "Product 3", 150m)
        };

        mockRepo.Setup(r => r.GetPagedAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        mockRepo.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

        // Act
        var result = await service.GetPagedAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        Assert.Equal("PROD-001", result.Items[0].Code);
        Assert.Equal(100m, result.Items[0].BasePrice);
    }

    [Fact]
    public async Task GetPagedAsync_Page2_ShouldCalculateCorrectSkip()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();

        mockRepo.Setup(r => r.GetPagedAsync(20, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        mockRepo.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

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

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

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

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

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

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

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

        mockRepo.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        mockRepo.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

        // Act
        var result = await service.GetPagedAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task GetPagedAsync_ProductsWithMultipleVariants_ShouldUseLowestPrice()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();

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
        mockRepo.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());

        // Act
        var result = await service.GetPagedAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(100m, result.Items[0].BasePrice);
        Assert.Equal(90m, result.Items[0].SalePrice);
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

