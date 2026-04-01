using BackgroundLogService.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Helpers.Attributes;
using SHNGearBE.Models.DTOs.Order;
using SHNGearBE.Models.Enums;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Order;
using System.Security.Claims;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogService<OrderController> _logService;

    public OrderController(IOrderService orderService, ILogService<OrderController> logService)
    {
        _orderService = orderService;
        _logService = logService;
    }

    private Guid? GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null)
            {
                return Unauthorized(new ApiResponse(ResponseType.Unauthorized));
            }

            var order = await _orderService.CreateOrderAsync(accountId.Value, request, cancellationToken);
            return Ok(new ApiResponse(order, ResponseType.Created));
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

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null)
            {
                return Unauthorized(new ApiResponse(ResponseType.Unauthorized));
            }

            var orders = await _orderService.GetMyOrdersAsync(accountId.Value, page, pageSize, cancellationToken);
            return Ok(new ApiResponse(orders));
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

    [HttpGet("my-orders/{id:guid}")]
    public async Task<IActionResult> GetMyOrderDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null)
            {
                return Unauthorized(new ApiResponse(ResponseType.Unauthorized));
            }

            var order = await _orderService.GetMyOrderByIdAsync(accountId.Value, id, cancellationToken);
            if (order == null)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(order));
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

    [HttpPatch("my-orders/{id:guid}/cancel")]
    public async Task<IActionResult> CancelMyOrder(Guid id, [FromBody] CancelOrderRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null)
            {
                return Unauthorized(new ApiResponse(ResponseType.Unauthorized));
            }

            var order = await _orderService.CancelMyOrderAsync(accountId.Value, id, request?.Reason, cancellationToken);
            return Ok(new ApiResponse(order, ResponseType.Updated));
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

    [HttpGet("admin")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderService.GetOrdersAsync(status, page, pageSize, cancellationToken);
            return Ok(new ApiResponse(orders));
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

    [HttpGet("admin/{id:guid}")]
    public async Task<IActionResult> GetOrderDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if (order == null)
            {
                return NotFound(new ApiResponse(ResponseType.NotFound));
            }

            return Ok(new ApiResponse(order));
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

    [HttpPatch("admin/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request.Status, cancellationToken);
            return Ok(new ApiResponse(order, ResponseType.Updated));
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
