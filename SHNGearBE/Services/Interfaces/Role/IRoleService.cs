using SHNGearBE.Models.DTOs.Role;

namespace SHNGearBE.Services.Interfaces.Role;

public interface IRoleService
{
    Task<RoleDto?> GetRoleByIdAsync(Guid roleId);
    Task<RoleDto?> GetRoleByNameAsync(string roleName);
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request);
    Task<RoleDto?> UpdateRoleAsync(Guid roleId, UpdateRoleRequestDto request);
    Task<bool> DeleteRoleAsync(Guid roleId);
    Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
}
