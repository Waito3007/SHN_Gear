using Moq;
using Xunit;
using BackgroundLogService.Abstractions;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface;
using SHNGearBE.Services;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Tests.UnitTests.ProductTests;

public class ProductServiceCreateTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.VariantSkuExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.GetTagsByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag>());

        var productId = Guid.NewGuid();
        mockRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => new Product
            {
                Id = id,
                Code = "TEST-001",
                Name = "Test Product",
                Slug = "test-product",
                CategoryId = Guid.NewGuid(),
                BrandId = Guid.NewGuid(),
                Category = new Category { Id = Guid.NewGuid(), Name = "Test Cat", Slug = "test-cat" },
                Brand = new Brand { Id = Guid.NewGuid(), Name = "Test Brand" },
                Variants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        Sku = "SKU-001",
                        Name = "Size M",
                        Quantity = 10,
                        SafetyStock = 2,
                        Prices = new List<ProductVariantPrice>
                        {
                            new ProductVariantPrice
                            {
                                Id = Guid.NewGuid(),
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
            });

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "TEST-001",
            Name = "Test Product",
            Slug = "test-product",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "SKU-001",
                    Name = "Size M",
                    Quantity = 10,
                    SafetyStock = 2,
                    BasePrice = 100m,
                    SalePrice = 90m,
                    Currency = "USD"
                }
            }
        };

        // Act
        var result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST-001", result.Code);
        Assert.Equal("test-product", result.Slug);
        Assert.Single(result.Variants);
        Assert.Equal(100m, result.Variants[0].BasePrice);
        Assert.Equal(90m, result.Variants[0].SalePrice);

        mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        mockUoW.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ShouldThrowAlreadyExists()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync("DUP-CODE", It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "DUP-CODE",
            Name = "Duplicate",
            Slug = "duplicate",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest
                {
                    Sku = "SKU-DUP",
                    BasePrice = 50m,
                    Quantity = 5,
                    SafetyStock = 1
                }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.AlreadyExists, ex.ResponseType);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_ShouldThrowAlreadyExists()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync(It.IsAny<string>(), "duplicate-slug", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "UNIQUE-CODE",
            Name = "Test",
            Slug = "duplicate-slug",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "SKU-TEST", BasePrice = 10m, Quantity = 1 }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_DuplicateVariantSku_ShouldThrowAlreadyExists()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        mockRepo.Setup(r => r.CodeOrSlugExistsAsync(It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        mockRepo.Setup(r => r.VariantSkuExistsAsync("DUP-SKU", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "DUP-SKU", BasePrice = 10m, Quantity = 1 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.AlreadyExists, ex.ResponseType);
    }

    [Fact]
    public async Task CreateAsync_NoVariants_ShouldThrowInvalidData()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidData, ex.ResponseType);
    }

    [Fact]
    public async Task CreateAsync_VariantWithNegativePrice_ShouldThrowPriceMustBePositive()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "SKU-1", BasePrice = -10m, Quantity = 1 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.PriceMustBePositive, ex.ResponseType);
    }

    [Fact]
    public async Task CreateAsync_SalePriceGreaterThanBase_ShouldThrowException()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "SKU-1", BasePrice = 100m, SalePrice = 150m, Quantity = 1 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.SalePriceCannotExceedOriginalPrice, ex.ResponseType);
    }

    [Fact]
    public async Task CreateAsync_NegativeStock_ShouldThrowException()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<ProductService>>();

        var service = new ProductService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateProductRequest
        {
            Code = "TEST",
            Name = "Test",
            Slug = "test",
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Variants = new List<ProductVariantRequest>
            {
                new ProductVariantRequest { Sku = "SKU-1", BasePrice = 100m, Quantity = -5 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(ResponseType.StockCannotBeNegative, ex.ResponseType);
    }
}
