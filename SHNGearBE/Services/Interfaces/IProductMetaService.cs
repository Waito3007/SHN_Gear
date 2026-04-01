using SHNGearBE.Models.DTOs.Product;

namespace SHNGearBE.Services.Interfaces;

public interface IProductMetaService
{
    Task<IReadOnlyList<CategoryOptionResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BrandOptionResponse>> GetBrandsAsync(CancellationToken cancellationToken = default);
}
