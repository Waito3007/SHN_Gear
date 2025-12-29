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

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null || entity.IsDelete == true)
        {
            return null;
        }
        return entity;
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

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDelete = true;
            entity.UpdateAt = DateTime.UtcNow;
        }
    }
}