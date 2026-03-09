using ProductEntity = SHNGearBE.Models.Entities.Product.Product;
using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Repositorys.Interface.Product;

public interface IProductRepository : IGenericRepository<ProductEntity>
{
    Task<ProductEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> CodeOrSlugExistsAsync(string code, string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductEntity>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductEntity>> SearchPagedAsync(string? searchTerm, Guid? categoryId, Guid? brandId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountFilteredAsync(string? searchTerm, Guid? categoryId, Guid? brandId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetTagsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task<bool> VariantSkuExistsAsync(string sku, Guid? excludeVariantId = null, CancellationToken cancellationToken = default);

    // ============ Cached methods for frequently accessed data ============

    /// <summary>
    /// Get featured products (cached in Redis)
    /// </summary>
    Task<IReadOnlyList<ProductEntity>> GetFeaturedProductsCachedAsync(int take = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top selling products (cached in Redis)
    /// </summary>
    Task<IReadOnlyList<ProductEntity>> GetTopSellingProductsCachedAsync(int take = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get newest products (cached in Redis)
    /// </summary>
    Task<IReadOnlyList<ProductEntity>> GetNewestProductsCachedAsync(int take = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get product by ID with full details (cached in Redis)
    /// </summary>
    Task<ProductEntity?> GetByIdWithDetailsCachedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get product by slug with full details (cached in Redis)
    /// </summary>
    Task<ProductEntity?> GetBySlugCachedAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all product caches including list caches and specific product cache
    /// Call this after Create/Update/Delete operations
    /// </summary>
    /// <param name="productId">Optional: specific product ID to invalidate</param>
    /// <param name="slug">Optional: product slug to invalidate</param>
    Task InvalidateProductCacheAsync(Guid? productId = null, string? slug = null);
}
