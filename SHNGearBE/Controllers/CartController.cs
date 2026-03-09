using BackgroundLogService.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Models.DTOs.Cart;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Cart;
using System.Security.Claims;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogService<CartController> _logService;

    public CartController(ICartService cartService, ILogService<CartController> logService)
    {
        _cartService = cartService;
        _logService = logService;
    }

    private Guid? GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>
    /// Get current user's cart
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var cart = await _cartService.GetCartAsync(accountId.Value, cancellationToken);
            return Ok(new ApiResponse(cart));
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

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var cart = await _cartService.AddItemAsync(accountId.Value, request, cancellationToken);
            return Ok(new ApiResponse(cart));
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

    /// <summary>
    /// Update item quantity in cart
    /// </summary>
    [HttpPut("items/{productVariantId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid productVariantId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var cart = await _cartService.UpdateItemQuantityAsync(accountId.Value, productVariantId, request, cancellationToken);
            return Ok(new ApiResponse(cart));
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

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{productVariantId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productVariantId, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var cart = await _cartService.RemoveItemAsync(accountId.Value, productVariantId, cancellationToken);
            return Ok(new ApiResponse(cart));
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

    /// <summary>
    /// Clear entire cart
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            await _cartService.ClearCartAsync(accountId.Value);
            return Ok(new ApiResponse(ResponseType.Deleted));
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }
}
