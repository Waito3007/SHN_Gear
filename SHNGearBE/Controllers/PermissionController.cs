using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Helpers.Attributes;
using SHNGearBE.Models.DTOs.Permission;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Permission;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(IPermissionService permissionService, ILogger<PermissionController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewPermissions)]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(new ApiResponse(permissions));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching permissions");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.ViewPermissions)]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        try
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(permission));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching permission");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPost]
    [RequirePermission(Permissions.CreatePermission)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequestDto request)
    {
        try
        {
            var permission = await _permissionService.CreatePermissionAsync(request);
            return CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, new ApiResponse(permission));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating permission");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPut("{id}")]
    [RequirePermission(Permissions.EditPermission)]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] UpdatePermissionRequestDto request)
    {
        try
        {
            var permission = await _permissionService.UpdatePermissionAsync(id, request);
            return Ok(new ApiResponse(permission));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating permission");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permissions.DeletePermission)]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        try
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(new { message = "Permission deleted successfully" }));
        }
        catch (ProjectException ex)
        {
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting permission");
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }
}
