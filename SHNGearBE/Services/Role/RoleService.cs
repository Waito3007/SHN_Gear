using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Role;
using SHNGearBE.Repositorys.Interface.Permission;
using SHNGearBE.Services.Interfaces.Role;
using SHNGearBE.UnitOfWork;
using RoleDto = SHNGearBE.Models.DTOs.Role.RoleDto;
using CreateRoleRequestDto = SHNGearBE.Models.DTOs.Role.CreateRoleRequestDto;
using UpdateRoleRequestDto = SHNGearBE.Models.DTOs.Role.UpdateRoleRequestDto;
using PermissionDto = SHNGearBE.Models.DTOs.Role.PermissionDto;

namespace SHNGearBE.Services.Role;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId)
    {
        var role = await _roleRepository.GetRoleWithPermissionsAsync(roleId);
        return role == null ? null : MapToRoleDto(role);
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
    {
        var role = await _roleRepository.GetByNameAsync(roleName);
        if (role == null) return null;

        role = await _roleRepository.GetRoleWithPermissionsAsync(role.Id);
        return role == null ? null : MapToRoleDto(role);
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => !r.IsDelete)
            .ToListAsync();

        return roles.Select(MapToRoleDto).ToList();
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request)
    {
        if (await _roleRepository.GetByNameAsync(request.Name) != null)
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Role with this name already exists");
        }

        var role = new Models.Entities.Account.Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreateAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _roleRepository.AddAsync(role);
            await _unitOfWork.SaveAsync();

            // Assign permissions
            if (request.PermissionIds != null && request.PermissionIds.Any())
            {
                foreach (var permissionId in request.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
            }

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return (await GetRoleByIdAsync(role.Id))!;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<RoleDto?> UpdateRoleAsync(Guid roleId, UpdateRoleRequestDto request)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Role not found");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var existingRole = await _roleRepository.GetByNameAsync(request.Name);
                if (existingRole != null && existingRole.Id != roleId)
                {
                    throw new ProjectException(ResponseType.AlreadyExists, "Role with this name already exists");
                }
                role.Name = request.Name;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                role.Description = request.Description;
            }

            role.UpdateAt = DateTime.UtcNow;
            await _roleRepository.UpdateAsync(role);

            // Update permissions if provided
            if (request.PermissionIds != null)
            {
                // Remove existing permissions
                var existingPermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .ToListAsync();

                _context.RolePermissions.RemoveRange(existingPermissions);

                // Add new permissions
                foreach (var permissionId in request.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
            }

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return await GetRoleByIdAsync(roleId);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            return false;
        }

        await _roleRepository.DeleteAsync(roleId);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
    {
        var existingPermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (existingPermission != null)
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Role already has this permission");
        }

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null)
        {
            return false;
        }

        _context.RolePermissions.Remove(rolePermission);
        await _unitOfWork.SaveAsync();
        return true;
    }

    private RoleDto MapToRoleDto(Models.Entities.Account.Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreateAt = role.CreateAt,
            UpdateAt = role.UpdateAt,
            Permissions = role.RolePermissions?
                .Where(rp => !rp.Permission.IsDelete)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description
                }).ToList() ?? new List<PermissionDto>()
        };
    }
}
