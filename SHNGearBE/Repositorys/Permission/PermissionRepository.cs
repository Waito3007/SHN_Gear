using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using PermissionEntity = SHNGearBE.Models.Entities.Account.Permission;
using SHNGearBE.Repositorys.Interface.Permission;

namespace SHNGearBE.Repositorys.Permission;

public class PermissionRepository : GenericRepository<PermissionEntity>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PermissionEntity?> GetByNameAsync(string permissionName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Name == permissionName && !p.IsDelete);
    }

    /// <summary>
    /// Get permissions by account ID as IQueryable for deferred execution
    /// </summary>
    public IQueryable<PermissionEntity> GetPermissionsByAccountIdQueryable(Guid accountId)
    {
        return _dbSet
            .Where(p => p.RolePermissions.Any(rp =>
                rp.Role.AccountRoles.Any(ar => ar.AccountId == accountId))
                && !p.IsDelete)
            .Distinct();
    }

    public async Task<bool> HasPermissionAsync(Guid accountId, string permissionName)
    {
        return await _dbSet
            .AnyAsync(p => p.Name == permissionName
                && p.RolePermissions.Any(rp =>
                    rp.Role.AccountRoles.Any(ar => ar.AccountId == accountId))
                && !p.IsDelete);
    }

    /// <summary>
    /// Get permission names by account ID as IQueryable for deferred execution
    /// </summary>
    public IQueryable<string> GetPermissionNamesByAccountIdQueryable(Guid accountId)
    {
        return _dbSet
            .Where(p => p.RolePermissions.Any(rp =>
                rp.Role.AccountRoles.Any(ar => ar.AccountId == accountId))
                && !p.IsDelete)
            .Select(p => p.Name)
            .Distinct();
    }
}
