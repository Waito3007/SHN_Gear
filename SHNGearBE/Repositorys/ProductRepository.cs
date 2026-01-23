using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Repositorys.Interface;

namespace SHNGearBE.Repositorys;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Inventory)
            .Include(p => p.Prices)
            .Include(p => p.Images)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ProductAttributes).ThenInclude(pa => pa.AttributeDefinition)
            .Include(p => p.Variants).ThenInclude(v => v.Prices)
            .Include(p => p.Variants).ThenInclude(v => v.VariantAttributes).ThenInclude(va => va.AttributeDefinition)
            .Where(p => !p.IsDelete && p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyList<Product>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
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
}
