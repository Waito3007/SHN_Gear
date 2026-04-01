using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Repositorys.Interface.Product;

namespace SHNGearBE.Repositorys.Product;

public class BrandRepository : GenericRepository<Brand>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Brand>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
