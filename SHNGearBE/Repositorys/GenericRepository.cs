using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.Entities;
using SHNGearBE.Repositorys.Interface;

namespace SHNGearBE.Repositorys;


public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id &&  e.isDelete == false);
    }

    public async Task<T> AddAsync(T Entity)
    {
        Entity.CreateAt = DateTime.UtcNow;
        await _dbSet.AddAsync(Entity);
        return Entity;
    }

    public async Task UpdateAsync(T Entity)
    {
        Entity.UpdateAt = DateTime.UtcNow;
        _dbSet.Update(Entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.isDelete = true;
            entity.UpdateAt = DateTime.UtcNow;
        }
    }
}