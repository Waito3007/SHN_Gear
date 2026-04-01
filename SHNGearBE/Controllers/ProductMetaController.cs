using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductMetaController : ControllerBase
{
    private readonly IProductMetaService _productMetaService;

    public ProductMetaController(IProductMetaService productMetaService)
    {
        _productMetaService = productMetaService;
    }

    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken = default)
    {
        var result = await _productMetaService.GetCategoriesAsync(cancellationToken);
        return Ok(new ApiResponse(result));
    }

    [AllowAnonymous]
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken = default)
    {
        var result = await _productMetaService.GetBrandsAsync(cancellationToken);
        return Ok(new ApiResponse(result));
    }
}
