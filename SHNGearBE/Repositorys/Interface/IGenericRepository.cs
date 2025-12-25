using SHNGearBE.Models.Entities;

namespace SHNGearBE.Repositorys.Interface;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T Entity);
    Task UpdateAsync(T Entity);
    Task DeleteAsync(int id);
}