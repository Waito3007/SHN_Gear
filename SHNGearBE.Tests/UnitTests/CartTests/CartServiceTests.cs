using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SHNGearBE.Data;
using SHNGearBE.Infrastructure.Redis;
using SHNGearBE.Models.DTOs.Cart;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Cart;
using Xunit;

namespace SHNGearBE.Tests.UnitTests.CartTests;

public class CartServiceTests
{
    [Fact]
    public async Task AddItemAsync_ValidRequest_ShouldSaveAndReturnEnrichedCart()
    {
        var accountId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await using var context = CreateDbContext();
        await SeedVariantAsync(context, variantId, quantity: 20, reserved: 0, safetyStock: 2);

        var cacheMock = new Mock<ICacheService>();
        var logMock = new Mock<ILogService<CartService>>();
        cacheMock.Setup(c => c.GetAsync<CartEntry>(It.IsAny<string>())).ReturnsAsync((CartEntry?)null);

        var service = new CartService(cacheMock.Object, context, logMock.Object);
        var request = new AddToCartRequest { ProductVariantId = variantId, Quantity = 2 };

        var result = await service.AddItemAsync(accountId, request, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(180m, result.TotalAmount);
        Assert.Equal(variantId, result.Items[0].ProductVariantId);
        cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<CartEntry>(), It.IsAny<TimeSpan?>()), Times.Once);
        logMock.Verify(l => l.WriteMessageAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_InvalidQuantity_ShouldThrowInvalidValue()
    {
        await using var context = CreateDbContext();
        var cacheMock = new Mock<ICacheService>();
        var logMock = new Mock<ILogService<CartService>>();
        var service = new CartService(cacheMock.Object, context, logMock.Object);

        var request = new AddToCartRequest { ProductVariantId = Guid.NewGuid(), Quantity = 0 };

        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.AddItemAsync(Guid.NewGuid(), request, CancellationToken.None));
        Assert.Equal(ResponseType.InvalidValue, ex.ResponseType);
    }

    [Fact]
    public async Task UpdateItemQuantityAsync_ZeroQuantity_ShouldRemoveItemAndClearCacheKey()
    {
        var accountId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        await using var context = CreateDbContext();
        var cacheMock = new Mock<ICacheService>();
        var logMock = new Mock<ILogService<CartService>>();

        cacheMock.Setup(c => c.GetAsync<CartEntry>(It.IsAny<string>()))
            .ReturnsAsync(new CartEntry
            {
                Items = new List<CartItemEntry>
                {
                    new CartItemEntry { ProductVariantId = variantId, Quantity = 3 }
                }
            });

        var service = new CartService(cacheMock.Object, context, logMock.Object);
        var request = new UpdateCartItemRequest { Quantity = 0 };

        var result = await service.UpdateItemQuantityAsync(accountId, variantId, request, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldRemoveCacheAndWriteLog()
    {
        await using var context = CreateDbContext();
        var cacheMock = new Mock<ICacheService>();
        var logMock = new Mock<ILogService<CartService>>();
        var accountId = Guid.NewGuid();

        var service = new CartService(cacheMock.Object, context, logMock.Object);

        await service.ClearCartAsync(accountId);

        cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        logMock.Verify(l => l.WriteMessageAsync(It.IsAny<string>()), Times.Once);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task SeedVariantAsync(ApplicationDbContext context, Guid variantId, int quantity, int reserved, int safetyStock)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category",
            Slug = "category"
        };

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = "Brand"
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = "P001",
            Name = "Product 1",
            Slug = "product-1",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Category = category,
            Brand = brand,
            IsDelete = false
        };

        var variant = new ProductVariant
        {
            Id = variantId,
            ProductId = product.Id,
            Product = product,
            Sku = "SKU-001",
            Name = "Default",
            Quantity = quantity,
            ReservedStock = reserved,
            SafetyStock = safetyStock
        };

        variant.Prices.Add(new ProductVariantPrice
        {
            Id = Guid.NewGuid(),
            ProductVariantId = variantId,
            Currency = "VND",
            BasePrice = 100m,
            SalePrice = 90m,
            ValidFrom = DateTime.UtcNow.AddDays(-1)
        });

        product.Images.Add(new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Url = "https://example.com/image.jpg",
            IsPrimary = true,
            SortOrder = 1
        });

        context.Categories.Add(category);
        context.Brands.Add(brand);
        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();
    }
}
