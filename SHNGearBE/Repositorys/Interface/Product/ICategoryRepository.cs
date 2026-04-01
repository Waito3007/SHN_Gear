using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Repositorys.Interface.Product;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
}
