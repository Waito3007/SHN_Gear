using PermissionEntity = SHNGearBE.Models.Entities.Account.Permission;

namespace SHNGearBE.Repositorys.Interface.Permission;

public interface IPermissionRepository : IGenericRepository<PermissionEntity>
{
    Task<PermissionEntity?> GetByNameAsync(string permissionName);

    /// <summary>
    /// Get permissions by account ID as IQueryable for deferred execution
    /// </summary>
    IQueryable<PermissionEntity> GetPermissionsByAccountIdQueryable(Guid accountId);

    Task<bool> HasPermissionAsync(Guid accountId, string permissionName);

    /// <summary>
    /// Get permission names by account ID as IQueryable for deferred execution
    /// </summary>
    IQueryable<string> GetPermissionNamesByAccountIdQueryable(Guid accountId);
}
