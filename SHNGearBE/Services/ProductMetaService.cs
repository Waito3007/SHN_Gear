using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Services.Interfaces;

namespace SHNGearBE.Services;

public class ProductMetaService : IProductMetaService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;

    public ProductMetaService(ICategoryRepository categoryRepository, IBrandRepository brandRepository)
    {
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
    }

    public async Task<IReadOnlyList<CategoryOptionResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetActiveAsync(cancellationToken);
        return categories
            .Select(x => new CategoryOptionResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug
            })
            .ToList();
    }

    public async Task<IReadOnlyList<BrandOptionResponse>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _brandRepository.GetActiveAsync(cancellationToken);
        return brands
            .Select(x => new BrandOptionResponse
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToList();
    }
}
