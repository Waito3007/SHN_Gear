using SHNGearBE.Models.DTOs.Product;

namespace SHNGearBE.Services.Interfaces.Product;

public interface IBrandService
{
    Task<BrandDto?> GetBrandByIdAsync(Guid id);
    Task<IEnumerable<BrandDto>> GetAllBrandsAsync();
    Task<IEnumerable<BrandDto>> GetActiveBrandsAsync();
    Task<BrandDto> CreateBrandAsync(CreateBrandRequest request);
    Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandRequest request);
    Task<bool> DeleteBrandAsync(Guid id);
}

public interface ICategoryService
{
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync();
    Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(Guid id);
}
