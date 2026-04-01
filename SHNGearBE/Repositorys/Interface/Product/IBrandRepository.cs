using SHNGearBE.Models.Entities.Product;

namespace SHNGearBE.Repositorys.Interface.Product;

public interface IBrandRepository : IGenericRepository<Brand>
{
    Task<IReadOnlyList<Brand>> GetActiveAsync(CancellationToken cancellationToken = default);
}
