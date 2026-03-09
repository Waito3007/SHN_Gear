using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Infrastructure.Redis;
using SHNGearBE.Models.DTOs.Cart;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Cart;

namespace SHNGearBE.Services.Cart;

public class CartService : ICartService
{
    private readonly ICacheService _cacheService;
    private readonly ApplicationDbContext _context;
    private readonly ILogService<CartService> _logService;

    private const string CartKeyPrefix = "cart:";
    private static readonly TimeSpan CartExpiration = TimeSpan.FromDays(30);
    private const int MaxItemsPerCart = 50;
    private const int MaxQuantityPerItem = 99;

    public CartService(ICacheService cacheService, ApplicationDbContext context, ILogService<CartService> logService)
    {
        _cacheService = cacheService;
        _context = context;
        _logService = logService;
    }

    public async Task<CartDto> GetCartAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var entry = await GetCartEntryAsync(accountId);
        return await EnrichCartAsync(accountId, entry, cancellationToken);
    }

    public async Task<CartDto> AddItemAsync(Guid accountId, AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
            throw new ProjectException(ResponseType.InvalidValue, "Số lượng phải lớn hơn 0");

        // Validate variant exists and has stock
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.Id == request.ProductVariantId && !v.Product.IsDelete)
            .FirstOrDefaultAsync(cancellationToken);

        if (variant == null)
            throw new ProjectException(ResponseType.NotFound, "Sản phẩm không tồn tại");

        var entry = await GetCartEntryAsync(accountId);

        var existing = entry.Items.FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);
        if (existing != null)
        {
            existing.Quantity += request.Quantity;
            if (existing.Quantity > MaxQuantityPerItem)
                existing.Quantity = MaxQuantityPerItem;
        }
        else
        {
            if (entry.Items.Count >= MaxItemsPerCart)
                throw new ProjectException(ResponseType.BadRequest, $"Giỏ hàng tối đa {MaxItemsPerCart} sản phẩm");

            entry.Items.Add(new CartItemEntry
            {
                ProductVariantId = request.ProductVariantId,
                Quantity = Math.Min(request.Quantity, MaxQuantityPerItem)
            });
        }

        // Validate against available stock
        var totalQuantity = entry.Items.First(i => i.ProductVariantId == request.ProductVariantId).Quantity;
        if (totalQuantity > variant.AvailableToSell)
        {
            entry.Items.First(i => i.ProductVariantId == request.ProductVariantId).Quantity = Math.Max(variant.AvailableToSell, 0);
            if (variant.AvailableToSell <= 0)
            {
                entry.Items.RemoveAll(i => i.ProductVariantId == request.ProductVariantId);
                throw new ProjectException(ResponseType.BadRequest, "Sản phẩm hết hàng");
            }
        }

        entry.UpdatedAt = DateTime.UtcNow;
        await SaveCartEntryAsync(accountId, entry);

        await _logService.WriteMessageAsync($"Item {request.ProductVariantId} added to cart for account {accountId}");
        return await EnrichCartAsync(accountId, entry, cancellationToken);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(Guid accountId, Guid productVariantId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 0)
            throw new ProjectException(ResponseType.InvalidValue, "Số lượng không được âm");

        var entry = await GetCartEntryAsync(accountId);
        var item = entry.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);

        if (item == null)
            throw new ProjectException(ResponseType.NotFound, "Sản phẩm không có trong giỏ hàng");

        if (request.Quantity == 0)
        {
            entry.Items.Remove(item);
        }
        else
        {
            // Validate stock
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Id == productVariantId && !v.Product.IsDelete)
                .FirstOrDefaultAsync(cancellationToken);

            if (variant == null)
            {
                entry.Items.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(request.Quantity, Math.Min(MaxQuantityPerItem, variant.AvailableToSell));
            }
        }

        entry.UpdatedAt = DateTime.UtcNow;
        await SaveCartEntryAsync(accountId, entry);

        return await EnrichCartAsync(accountId, entry, cancellationToken);
    }

    public async Task<CartDto> RemoveItemAsync(Guid accountId, Guid productVariantId, CancellationToken cancellationToken = default)
    {
        var entry = await GetCartEntryAsync(accountId);
        entry.Items.RemoveAll(i => i.ProductVariantId == productVariantId);

        entry.UpdatedAt = DateTime.UtcNow;
        await SaveCartEntryAsync(accountId, entry);

        await _logService.WriteMessageAsync($"Item {productVariantId} removed from cart for account {accountId}");
        return await EnrichCartAsync(accountId, entry, cancellationToken);
    }

    public async Task ClearCartAsync(Guid accountId)
    {
        await _cacheService.RemoveAsync(GetCartKey(accountId));
        await _logService.WriteMessageAsync($"Cart cleared for account {accountId}");
    }

    // ============ Private helpers ============

    private static string GetCartKey(Guid accountId) => $"{CartKeyPrefix}{accountId}";

    private async Task<CartEntry> GetCartEntryAsync(Guid accountId)
    {
        return await _cacheService.GetAsync<CartEntry>(GetCartKey(accountId)) ?? new CartEntry();
    }

    private async Task SaveCartEntryAsync(Guid accountId, CartEntry entry)
    {
        if (entry.Items.Count == 0)
        {
            await _cacheService.RemoveAsync(GetCartKey(accountId));
            return;
        }

        await _cacheService.SetAsync(GetCartKey(accountId), entry, CartExpiration);
    }

    /// <summary>
    /// Enrich the minimal Redis cart entry with full product info from DB
    /// </summary>
    private async Task<CartDto> EnrichCartAsync(Guid accountId, CartEntry entry, CancellationToken cancellationToken)
    {
        var cart = new CartDto
        {
            AccountId = accountId,
            UpdatedAt = entry.UpdatedAt
        };

        if (entry.Items.Count == 0)
            return cart;

        var variantIds = entry.Items.Select(i => i.ProductVariantId).ToList();

        var variants = await _context.ProductVariants
            .Include(v => v.Product).ThenInclude(p => p.Images)
            .Include(v => v.Prices)
            .Where(v => variantIds.Contains(v.Id) && !v.Product.IsDelete)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var variantMap = variants.ToDictionary(v => v.Id);
        var itemsToRemove = new List<Guid>();

        foreach (var item in entry.Items)
        {
            if (!variantMap.TryGetValue(item.ProductVariantId, out var variant))
            {
                // Product/variant was deleted, mark for removal
                itemsToRemove.Add(item.ProductVariantId);
                continue;
            }

            var activePrice = variant.Prices
                .Where(p => !p.IsDelete && p.ValidFrom <= DateTime.UtcNow && (p.ValidTo == null || p.ValidTo > DateTime.UtcNow))
                .OrderByDescending(p => p.ValidFrom)
                .FirstOrDefault();

            var unitPrice = activePrice?.SalePrice ?? activePrice?.BasePrice ?? 0;
            var primaryImage = variant.Product.Images.FirstOrDefault(i => i.IsPrimary && !i.IsDelete)
                               ?? variant.Product.Images.FirstOrDefault(i => !i.IsDelete);

            cart.Items.Add(new CartItemDto
            {
                ProductVariantId = variant.Id,
                ProductName = variant.Product.Name,
                VariantName = variant.Name ?? variant.Sku,
                Sku = variant.Sku,
                ImageUrl = primaryImage?.Url,
                UnitPrice = unitPrice,
                Currency = activePrice?.Currency ?? "VND",
                Quantity = item.Quantity,
                SubTotal = unitPrice * item.Quantity,
                AvailableStock = variant.AvailableToSell
            });
        }

        // Clean up deleted variants from cart
        if (itemsToRemove.Count > 0)
        {
            entry.Items.RemoveAll(i => itemsToRemove.Contains(i.ProductVariantId));
            entry.UpdatedAt = DateTime.UtcNow;
            await SaveCartEntryAsync(accountId, entry);
        }

        cart.TotalAmount = cart.Items.Sum(i => i.SubTotal);
        cart.TotalItems = cart.Items.Sum(i => i.Quantity);

        return cart;
    }
}
