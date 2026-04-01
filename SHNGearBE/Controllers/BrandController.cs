using BackgroundLogService.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Helpers.Attributes;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Product;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly ILogService<BrandController> _logService;

    public BrandController(IBrandService brandService, ILogService<BrandController> logService)
    {
        _brandService = brandService;
        _logService = logService;
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewBrands)]
    public async Task<IActionResult> GetAllBrands()
    {
        try
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return Ok(new ApiResponse(brands));
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

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveBrands()
    {
        try
        {
            var brands = await _brandService.GetActiveBrandsAsync();
            return Ok(new ApiResponse(brands));
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.ViewBrands)]
    public async Task<IActionResult> GetBrandById(Guid id)
    {
        try
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(brand));
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
    [RequirePermission(Permissions.ManageBrands)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request)
    {
        try
        {
            var brand = await _brandService.CreateBrandAsync(request);
            return CreatedAtAction(nameof(GetBrandById), new { id = brand.Id }, new ApiResponse(brand));
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
    [RequirePermission(Permissions.ManageBrands)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request)
    {
        try
        {
            var brand = await _brandService.UpdateBrandAsync(id, request);
            return Ok(new ApiResponse(brand));
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
    [RequirePermission(Permissions.ManageBrands)]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        try
        {
            var result = await _brandService.DeleteBrandAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(new { message = "Brand deleted successfully" }));
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
