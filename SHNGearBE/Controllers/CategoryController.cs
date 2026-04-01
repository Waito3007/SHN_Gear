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
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogService<CategoryController> _logService;

    public CategoryController(ICategoryService categoryService, ILogService<CategoryController> logService)
    {
        _categoryService = categoryService;
        _logService = logService;
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewCategories)]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new ApiResponse(categories));
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
    public async Task<IActionResult> GetActiveCategories()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(new ApiResponse(categories));
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryTree()
    {
        try
        {
            var tree = await _categoryService.GetCategoryTreeAsync();
            return Ok(new ApiResponse(tree));
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.ViewCategories)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(category));
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
    [RequirePermission(Permissions.ManageCategories)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, new ApiResponse(category));
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
    [RequirePermission(Permissions.ManageCategories)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, request);
            return Ok(new ApiResponse(category));
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
    [RequirePermission(Permissions.ManageCategories)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(new { message = "Category deleted successfully" }));
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
