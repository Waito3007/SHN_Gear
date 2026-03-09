using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using RoleEntity = SHNGearBE.Models.Entities.Account.Role;
using PermissionEntity = SHNGearBE.Models.Entities.Account.Permission;
using SHNGearBE.Repositorys.Interface.Role;

namespace SHNGearBE.Repositorys.Role;

public class RoleRepository : GenericRepository<RoleEntity>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RoleEntity?> GetByNameAsync(string roleName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.Name == roleName && !r.IsDelete);
    }

    public async Task<RoleEntity?> GetRoleWithPermissionsAsync(Guid roleId)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDelete);
    }

    /// <summary>
    /// Get roles by account ID as IQueryable for deferred execution
    /// </summary>
    public IQueryable<RoleEntity> GetRolesByAccountIdQueryable(Guid accountId)
    {
        return _dbSet
            .Where(r => r.AccountRoles.Any(ar => ar.AccountId == accountId) && !r.IsDelete);
    }

    /// <summary>
    /// Get permissions by role ID as IQueryable for deferred execution
    /// </summary>
    public IQueryable<PermissionEntity> GetPermissionsByRoleIdQueryable(Guid roleId)
    {
        return _context.Set<PermissionEntity>()
            .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId) && !p.IsDelete);
    }
}
