using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Repositorys.Interface;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> CodeOrSlugExistsAsync(string code, string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetTagsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task<bool> VariantSkuExistsAsync(string sku, Guid? excludeVariantId = null, CancellationToken cancellationToken = default);
}
