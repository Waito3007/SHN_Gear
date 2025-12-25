namespace SHNGearBE.Services.Interfaces;

public interface IBaseService<TEntity, TDto, TCreateRequest, TUpdateRequest>
{
    Task<TDto?> GetByIdAsync(int id);
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto> CreateAsync(TCreateRequest request);
    Task<TDto?> UpdateAsync(int id, TUpdateRequest request);
    Task<bool> DeleteAsync(int id);
}
