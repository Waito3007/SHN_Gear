using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Helpers.Attributes;
using SHNGearBE.Models.DTOs.Role;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Role;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewRoles)]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(new ApiResponse(roles));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching roles");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.ViewRoles)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(role));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching role");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPost]
    [RequirePermission(Permissions.CreateRole)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, new ApiResponse(role));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating role");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPut("{id}")]
    [RequirePermission(Permissions.EditRole)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequestDto request)
    {
        try
        {
            var role = await _roleService.UpdateRoleAsync(id, request);
            return Ok(new ApiResponse(role));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating role");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permissions.DeleteRole)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(new { message = "Role deleted successfully" }));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting role");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    [RequirePermission(Permissions.ManageRolePermissions)]
    public async Task<IActionResult> AssignPermissionToRole(Guid roleId, Guid permissionId)
    {
        try
        {
            var result = await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
            return Ok(new ApiResponse(new { message = "Permission assigned successfully" }));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning permission");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [RequirePermission(Permissions.ManageRolePermissions)]
    public async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        try
        {
            var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
            if (!result)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(new { message = "Permission removed successfully" }));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing permission");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }
}
