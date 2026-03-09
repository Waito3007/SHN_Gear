using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.DTOs.Permission;
using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Permission;
using SHNGearBE.Services.Interfaces.Permission;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Services.Permission;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(
        IPermissionRepository permissionRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(Guid permissionId)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        return permission == null ? null : MapToPermissionDto(permission);
    }

    public async Task<PermissionDto?> GetPermissionByNameAsync(string permissionName)
    {
        var permission = await _permissionRepository.GetByNameAsync(permissionName);
        return permission == null ? null : MapToPermissionDto(permission);
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .Where(p => !p.IsDelete)
            .ToListAsync();

        return permissions.Select(MapToPermissionDto).ToList();
    }

    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequestDto request)
    {
        if (await _permissionRepository.GetByNameAsync(request.Name) != null)
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Permission with this name already exists");
        }

        var permission = new Models.Entities.Account.Permission
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreateAt = DateTime.UtcNow
        };

        await _permissionRepository.AddAsync(permission);
        await _unitOfWork.SaveAsync();

        return MapToPermissionDto(permission);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(Guid permissionId, UpdatePermissionRequestDto request)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Permission not found");
        }

        if (!string.IsNullOrEmpty(request.Name) && request.Name != permission.Name)
        {
            var existingPermission = await _permissionRepository.GetByNameAsync(request.Name);
            if (existingPermission != null)
            {
                throw new ProjectException(ResponseType.AlreadyExists, "Permission with this name already exists");
            }
            permission.Name = request.Name;
        }

        if (request.Description != null)
        {
            permission.Description = request.Description;
        }

        permission.UpdateAt = DateTime.UtcNow;

        await _permissionRepository.UpdateAsync(permission);
        await _unitOfWork.SaveAsync();

        return MapToPermissionDto(permission);
    }

    public async Task<bool> DeletePermissionAsync(Guid permissionId)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null)
        {
            return false;
        }

        await _permissionRepository.DeleteAsync(permissionId);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> HasPermissionAsync(Guid accountId, string permissionName)
    {
        return await _permissionRepository.HasPermissionAsync(accountId, permissionName);
    }

    private PermissionDto MapToPermissionDto(Models.Entities.Account.Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            CreateAt = permission.CreateAt,
            UpdateAt = permission.UpdateAt
        };
    }
}
