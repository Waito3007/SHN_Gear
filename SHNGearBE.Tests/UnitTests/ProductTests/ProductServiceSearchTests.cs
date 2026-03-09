using Moq;
using Xunit;
using BackgroundLogService.Abstractions;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Services;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Tests.UnitTests.ProductTests;

public class ProductServiceSearchTests
{
    [Fact]
    public async Task SearchAsync_WithSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var products = new List<Product>
        {
            CreateTestProduct("LAPTOP-001", "Gaming Laptop", 1500m),
            CreateTestProduct("LAPTOP-002", "Business Laptop", 1200m)
        };

        var request = new ProductFilterRequest
        {
            SearchTerm = "laptop",
            Page = 1,
            PageSize = 10
        };

        mockRepo.Setup(r => r.SearchPagedAsync("laptop", null, null, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        mockRepo.Setup(r => r.CountFilteredAsync("laptop", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.All(result.Items, item => Assert.Contains("Laptop", item.Name));
    }

    [Fact]
    public async Task SearchAsync_WithCategoryId_ShouldReturnProductsInCategory()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            CreateTestProduct("PROD-001", "Product 1", 100m, categoryId),
            CreateTestProduct("PROD-002", "Product 2", 200m, categoryId)
        };

        var request = new ProductFilterRequest
        {
            CategoryId = categoryId,
            Page = 1,
            PageSize = 10
        };

        mockRepo.Setup(r => r.SearchPagedAsync(null, categoryId, null, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        mockRepo.Setup(r => r.CountFilteredAsync(null, categoryId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_WithBrandId_ShouldReturnProductsFromBrand()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var brandId = Guid.NewGuid();
        var products = new List<Product>
        {
            CreateTestProduct("PHONE-001", "Phone 1", 800m, brandId: brandId),
            CreateTestProduct("PHONE-002", "Phone 2", 900m, brandId: brandId)
        };

        var request = new ProductFilterRequest
        {
            BrandId = brandId,
            Page = 1,
            PageSize = 10
        };

        mockRepo.Setup(r => r.SearchPagedAsync(null, null, brandId, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        mockRepo.Setup(r => r.CountFilteredAsync(null, null, brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_CombinedFilters_ShouldReturnFilteredProducts()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var products = new List<Product>
        {
            CreateTestProduct("GAMING-001", "Gaming Mouse", 50m, categoryId, brandId)
        };

        var request = new ProductFilterRequest
        {
            SearchTerm = "gaming",
            CategoryId = categoryId,
            BrandId = brandId,
            Page = 1,
            PageSize = 10
        };

        mockRepo.Setup(r => r.SearchPagedAsync("gaming", categoryId, brandId, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        mockRepo.Setup(r => r.CountFilteredAsync("gaming", categoryId, brandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.Count);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Contains("Gaming", result.Items[0].Name);
    }

    [Fact]
    public async Task SearchAsync_NoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var request = new ProductFilterRequest
        {
            SearchTerm = "nonexistent",
            Page = 1,
            PageSize = 10
        };

        mockRepo.Setup(r => r.SearchPagedAsync("nonexistent", null, null, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        mockRepo.Setup(r => r.CountFilteredAsync("nonexistent", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task SearchAsync_InvalidPage_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var request = new ProductFilterRequest
        {
            Page = 0,
            PageSize = 10
        };

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() =>
            service.SearchAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task SearchAsync_InvalidPageSize_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var request = new ProductFilterRequest
        {
            Page = 1,
            PageSize = -1
        };

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() =>
            service.SearchAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task SearchAsync_SecondPage_ShouldCalculateCorrectSkip()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var request = new ProductFilterRequest
        {
            Page = 2,
            PageSize = 5
        };

        mockRepo.Setup(r => r.SearchPagedAsync(null, null, null, 5, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        mockRepo.Setup(r => r.CountFilteredAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        await service.SearchAsync(request, CancellationToken.None);

        // Assert - Verify skip = (page-1) * pageSize = 1 * 5 = 5
        mockRepo.Verify(r => r.SearchPagedAsync(null, null, null, 5, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectMetadata()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var products = new List<Product>
        {
            CreateTestProduct("PROD-001", "Product 1", 100m),
            CreateTestProduct("PROD-002", "Product 2", 200m)
        };

        var request = new ProductFilterRequest
        {
            Page = 2,
            PageSize = 2
        };

        mockRepo.Setup(r => r.SearchPagedAsync(null, null, null, 2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        mockRepo.Setup(r => r.CountFilteredAsync(null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        var result = await service.SearchAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(5, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    private static Product CreateTestProduct(string code, string name, decimal price, Guid? categoryId = null, Guid? brandId = null)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Slug = name.ToLowerInvariant().Replace(" ", "-"),
            CategoryId = categoryId ?? Guid.NewGuid(),
            BrandId = brandId ?? Guid.NewGuid(),
            Category = new Category { Id = categoryId ?? Guid.NewGuid(), Name = "Test Category", Slug = "test-category" },
            Brand = new Brand { Id = brandId ?? Guid.NewGuid(), Name = "Test Brand" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Sku = $"{code}-SKU",
                    Quantity = 10,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice
                        {
                            BasePrice = price,
                            SalePrice = price * 0.9m,
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
