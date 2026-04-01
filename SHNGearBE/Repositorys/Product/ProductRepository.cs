using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using ProductEntity = SHNGearBE.Models.Entities.Product.Product;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Infrastructure.Redis;

namespace SHNGearBE.Repositorys.Product;

public class ProductRepository : GenericRepository<ProductEntity>, IProductRepository
{
    private readonly ICacheService _cacheService;

    // Cache keys
    private const string FeaturedProductsCacheKey = "products:featured";
    private const string TopSellingProductsCacheKey = "products:topselling";
    private const string NewestProductsCacheKey = "products:newest";
    private const string ProductDetailCacheKey = "products:detail";
    private const string ProductSlugCacheKey = "products:slug";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DetailCacheExpiration = TimeSpan.FromMinutes(30);

    public ProductRepository(ApplicationDbContext context, ICacheService cacheService) : base(context)
    {
        _cacheService = cacheService;
    }

    public async Task<ProductEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProductAttributes).ThenInclude(pa => pa.AttributeDefinition)
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .Include(p => p.Variants).ThenInclude(v => v.VariantAttributes).ThenInclude(va => va.AttributeDefinition)
            .AsSplitQuery()
            .Where(p => !p.IsDelete && p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => !p.IsDelete && p.Slug == slug)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CodeOrSlugExistsAsync(string code, string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => !p.IsDelete)
            .Where(p => p.Code == code || p.Slug == slug)
            .Where(p => excludeProductId == null || p.Id != excludeProductId)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .Where(p => !p.IsDelete)
            .OrderByDescending(p => p.CreateAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductEntity>> SearchPagedAsync(string? searchTerm, Guid? categoryId, Guid? brandId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .Where(p => !p.IsDelete)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Code.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        return await query
            .OrderByDescending(p => p.CreateAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountFilteredAsync(string? searchTerm, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => !p.IsDelete).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Code.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (brandId.HasValue)
        {
            query = query.Where(p => p.BrandId == brandId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<ProductEntity>()
            .Where(p => !p.IsDelete)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetTagsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var normalizedNames = names.Select(n => n.Trim().ToLowerInvariant()).ToList();
        return await _context.Tags
            .Where(t => normalizedNames.Contains(t.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> VariantSkuExistsAsync(string sku, Guid? excludeVariantId = null, CancellationToken cancellationToken = default)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => !v.Product.IsDelete)
            .Where(v => v.Sku == sku)
            .Where(v => excludeVariantId == null || v.Id != excludeVariantId)
            .AnyAsync(cancellationToken);
    }

    // ============ Cached methods ============

    /// <summary>
    /// Get featured products with Redis caching
    /// </summary>
    public async Task<IReadOnlyList<ProductEntity>> GetFeaturedProductsCachedAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{FeaturedProductsCacheKey}:{take}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Variants).ThenInclude(v => v.Prices)
                .Include(p => p.Images)
                .Where(p => !p.IsDelete && p.IsFeatured)
                .OrderByDescending(p => p.CreateAt)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, CacheExpiration) ?? new List<ProductEntity>();
    }

    /// <summary>
    /// Get top selling products with Redis caching
    /// </summary>
    public async Task<IReadOnlyList<ProductEntity>> GetTopSellingProductsCachedAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{TopSellingProductsCacheKey}:{take}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Variants).ThenInclude(v => v.Prices)
                .Include(p => p.Images)
                .Where(p => !p.IsDelete)
                .OrderByDescending(p => p.SoldCount)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, CacheExpiration) ?? new List<ProductEntity>();
    }

    /// <summary>
    /// Get newest products with Redis caching
    /// </summary>
    public async Task<IReadOnlyList<ProductEntity>> GetNewestProductsCachedAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{NewestProductsCacheKey}:{take}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            return await _dbSet
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Variants).ThenInclude(v => v.Prices)
                .Include(p => p.Images)
                .Where(p => !p.IsDelete)
                .OrderByDescending(p => p.CreateAt)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }, CacheExpiration) ?? new List<ProductEntity>();
    }

    /// <summary>
    /// Invalidate all product caches
    /// </summary>
    public async Task InvalidateProductCacheAsync(Guid? productId = null, string? slug = null)
    {
        // Remove cached product lists with common take sizes
        foreach (var take in new[] { 5, 10, 15, 20, 50 })
        {
            await _cacheService.RemoveAsync($"{FeaturedProductsCacheKey}:{take}");
            await _cacheService.RemoveAsync($"{TopSellingProductsCacheKey}:{take}");
            await _cacheService.RemoveAsync($"{NewestProductsCacheKey}:{take}");
        }

        // Remove specific product detail cache if productId provided
        if (productId.HasValue)
        {
            var detailCacheKey = $"{ProductDetailCacheKey}:{productId.Value}";
            await _cacheService.RemoveAsync(detailCacheKey);
        }

        // Remove specific product slug cache if slug provided
        if (!string.IsNullOrWhiteSpace(slug))
        {
            var slugCacheKey = $"{ProductSlugCacheKey}:{slug.ToLowerInvariant()}";
            await _cacheService.RemoveAsync(slugCacheKey);
        }
    }

    /// <summary>
    /// Get product by ID with full details (cached in Redis)
    /// </summary>
    public async Task<ProductEntity?> GetByIdWithDetailsCachedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductDetailCacheKey}:{id}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            return await GetByIdWithDetailsAsync(id, cancellationToken);
        }, DetailCacheExpiration);
    }

    /// <summary>
    /// Get product by slug with full details (cached in Redis)
    /// </summary>
    public async Task<ProductEntity?> GetBySlugCachedAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.ToLowerInvariant();
        var cacheKey = $"{ProductSlugCacheKey}:{normalizedSlug}";

        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var product = await GetBySlugAsync(slug, cancellationToken);
            if (product == null) return null;

            // Get full details
            return await GetByIdWithDetailsAsync(product.Id, cancellationToken);
        }, DetailCacheExpiration);
    }
}
