using SHNGearBE.Models.DTOs.Permission;

namespace SHNGearBE.Services.Interfaces.Permission;

public interface IPermissionService
{
    Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId);
    Task<PermissionDto?> GetPermissionByNameAsync(string permissionName);
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequestDto request);
    Task<PermissionDto> UpdatePermissionAsync(Guid permissionId, UpdatePermissionRequestDto request);
    Task<bool> DeletePermissionAsync(Guid permissionId);
    Task<bool> HasPermissionAsync(Guid accountId, string permissionName);
}
