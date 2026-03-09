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

public class ProductServiceUpdateTests
{
    [Fact]
    public async Task UpdateAsync_ValidRequest_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var existingProduct = new Product
        {
            Id = productId,
            Code = "OLD-CODE",
            Name = "Old Name",
            Slug = "old-slug",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = "cat" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Brand" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = variantId,
                    Sku = "OLD-SKU",
                    Quantity = 5,
                    SafetyStock = 1,
                    Prices = new List<ProductVariantPrice>
                    {
                        new ProductVariantPrice { Id = Guid.NewGuid(), BasePrice = 50m, ValidFrom = DateTime.UtcNow }
                    },
                    VariantAttributes = new List<ProductVariantAttribute>()
                }
            },
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync("NEW-CODE", "new-slug", productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.VariantSkuExistsAsync("NEW-SKU", variantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.GetTagsByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag>());

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new UpdateProductRequest
        {
            Id = productId,
            Code = "NEW-CODE",
            Name = "New Name",
            Slug = "new-slug",
            CategoryId = existingProduct.CategoryId,
            BrandId = existingProduct.BrandId,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Id = variantId,
                    Sku = "NEW-SKU",
                    Quantity = 20,
                    SafetyStock = 3,
                    BasePrice = 120m,
                    SalePrice = 100m
                }
            }
        };

        // Act
        var result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NEW-CODE", result.Code);
        Assert.Equal("new-slug", result.Slug);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ShouldThrowProductNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "SKU", BasePrice = 10m, Quantity = 1 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.UpdateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.ProductNotFound, ex.ResponseType);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateCodeOrSlug_ShouldThrowAlreadyExists()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = productId,
                Code = "OLD",
                Slug = "old",
                CategoryId = Guid.NewGuid(),
                BrandId = Guid.NewGuid(),
                Category = new Category { Id = Guid.NewGuid(), Name = "C", Slug = "c" },
                Brand = new Brand { Id = Guid.NewGuid(), Name = "B" },
                Variants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        Sku = "SKU",
                        Quantity = 1,
                        Prices = new List<ProductVariantPrice> { new ProductVariantPrice { BasePrice = 10m, ValidFrom = DateTime.UtcNow } },
                        VariantAttributes = new List<ProductVariantAttribute>()
                    }
                },
                Images = new List<ProductImage>(),
                ProductTags = new List<ProductTag>(),
                ProductAttributes = new List<ProductAttribute>()
            });

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync("DUP-CODE", It.IsAny<string>(), productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new UpdateProductRequest
        {
            Id = productId,
            Code = "DUP-CODE",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "SKU-NEW", BasePrice = 10m, Quantity = 1 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.UpdateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.AlreadyExists, ex.ResponseType);
    }

    [Fact]
    public async Task UpdateAsync_AddNewVariant_ShouldAddVariantToProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingVariantId = Guid.NewGuid();

        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var existingProduct = new Product
        {
            Id = productId,
            Code = "PROD",
            Name = "Product",
            Slug = "product",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Category = new Category { Id = Guid.NewGuid(), Name = "Cat", Slug = "cat" },
            Brand = new Brand { Id = Guid.NewGuid(), Name = "Brand" },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = existingVariantId,
                    Sku = "EXIST-SKU",
                    Quantity = 5,
                    Prices = new List<ProductVariantPrice> { new ProductVariantPrice { BasePrice = 50m, ValidFrom = DateTime.UtcNow } },
                    VariantAttributes = new List<ProductVariantAttribute>()
                }
            },
            Images = new List<ProductImage>(),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = new List<ProductAttribute>()
        };

        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync(It.IsAny<string>(), It.IsAny<string>(), productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.VariantSkuExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.GetTagsByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag>());

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new UpdateProductRequest
        {
            Id = productId,
            Code = "PROD",
            Name = "Product",
            Slug = "product",
            CategoryId = existingProduct.CategoryId,
            BrandId = existingProduct.BrandId,
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Id = existingVariantId,
                    Sku = "EXIST-SKU",
                    BasePrice = 50m,
                    Quantity = 5
                },
                new ProductVariantRequest
                {
                    Sku = "NEW-SKU",
                    Name = "New Variant",
                    BasePrice = 80m,
                    Quantity = 10,
                    SafetyStock = 2
                }
            }
        };

        // Act
        var result = await service.UpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Variants.Count);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
    }
}
