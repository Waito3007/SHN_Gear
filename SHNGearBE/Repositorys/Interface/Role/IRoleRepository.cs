using RoleEntity = SHNGearBE.Models.Entities.Account.Role;
using PermissionEntity = SHNGearBE.Models.Entities.Account.Permission;

namespace SHNGearBE.Repositorys.Interface.Role;

public interface IRoleRepository : IGenericRepository<RoleEntity>
{
    Task<RoleEntity?> GetByNameAsync(string roleName);
    Task<RoleEntity?> GetRoleWithPermissionsAsync(Guid roleId);

    /// <summary>
    /// Get roles by account ID as IQueryable for deferred execution
    /// </summary>
    IQueryable<RoleEntity> GetRolesByAccountIdQueryable(Guid accountId);

    /// <summary>
    /// Get permissions by role ID as IQueryable for deferred execution
    /// </summary>
    IQueryable<PermissionEntity> GetPermissionsByRoleIdQueryable(Guid roleId);
}
