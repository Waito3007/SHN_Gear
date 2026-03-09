using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Helpers.Attributes;
using SHNGearBE.Models.DTOs.Common;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Services.Interfaces;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemResponse>>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDetailResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await _productService.GetBySlugAsync(slug, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ProductListItemResponse>>> Search([FromQuery] ProductFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _productService.SearchAsync(filter, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<PagedResult<ProductListItemResponse>>> GetByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var filter = new ProductFilterRequest { CategoryId = categoryId, Page = page, PageSize = pageSize };
        var result = await _productService.SearchAsync(filter, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("brand/{brandId:guid}")]
    public async Task<ActionResult<PagedResult<ProductListItemResponse>>> GetByBrand(Guid brandId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var filter = new ProductFilterRequest { BrandId = brandId, Page = page, PageSize = pageSize };
        var result = await _productService.SearchAsync(filter, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [RequirePermission(Permissions.CreateProduct)]
    [HttpPost]
    public async Task<ActionResult<ProductDetailResponse>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var created = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [RequirePermission(Permissions.EditProduct)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDetailResponse>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest("Id không khớp với request body");
        }

        var updated = await _productService.UpdateAsync(request, cancellationToken);
        return Ok(updated);
    }

    [RequirePermission(Permissions.DeleteProduct)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
