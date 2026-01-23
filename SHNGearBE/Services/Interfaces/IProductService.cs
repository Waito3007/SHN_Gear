using SHNGearBE.Models.DTOs.Product;

namespace SHNGearBE.Services.Interfaces;

public interface IProductService
{
    Task<ProductDetailResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDetailResponse> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListItemResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
