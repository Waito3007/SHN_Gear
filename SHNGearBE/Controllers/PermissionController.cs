using BackgroundLogService.Abstractions;
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
    private readonly ILogService<PermissionController> _logService;

    public PermissionController(IPermissionService permissionService, ILogService<PermissionController> logService)
    {
        _permissionService = permissionService;
        _logService = logService;
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
            await _logService.WriteExceptionAsync(ex);
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
            await _logService.WriteExceptionAsync(ex);
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
            await _logService.WriteExceptionAsync(ex);
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
            await _logService.WriteExceptionAsync(ex);
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
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }
}
