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

public class ProductServiceDeleteTests
{
    [Fact]
    public async Task DeleteAsync_ValidId_ShouldSoftDeleteProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var product = new Product
        {
            Id = productId,
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            IsDelete = false,
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = "cat" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Brand" },
            Variants = new List<ProductVariant>(),
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        await service.DeleteAsync(productId, CancellationToken.None);

        // Assert
        Assert.True(product.IsDelete);
        Assert.NotNull(product.UpdateAt);
        mockUoW.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProductNotFound_ShouldThrowProductNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
        Assert.Equal(ResponseType.ProductNotFound, ex.ResponseType);
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeleted_ShouldStillProcessSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var product = new Product
        {
            Id = productId,
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            IsDelete = true,
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = "cat" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Brand" },
            Variants = new List<ProductVariant>(),
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        // Act
        await service.DeleteAsync(productId, CancellationToken.None);

        // Assert
        Assert.True(product.IsDelete);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
    }
}
