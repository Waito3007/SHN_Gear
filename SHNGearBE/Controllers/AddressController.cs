using BackgroundLogService.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Models.DTOs.Address;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Address;
using System.Security.Claims;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ILogService<AddressController> _logService;

    public AddressController(IAddressService addressService, ILogService<AddressController> logService)
    {
        _addressService = addressService;
        _logService = logService;
    }

    private Guid? GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAddresses(CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var addresses = await _addressService.GetByAccountIdAsync(accountId.Value, cancellationToken);
            return Ok(new ApiResponse(addresses));
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var address = await _addressService.GetByIdAsync(id, accountId.Value, cancellationToken);
            if (address == null) return NotFound(new ApiResponse(ResponseType.NotFound));

            return Ok(new ApiResponse(address));
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
    public async Task<IActionResult> Create([FromBody] CreateAddressRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var address = await _addressService.CreateAsync(accountId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = address.Id }, new ApiResponse(address, ResponseType.Created));
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAddressRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var address = await _addressService.UpdateAsync(id, accountId.Value, request, cancellationToken);
            return Ok(new ApiResponse(address));
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            await _addressService.DeleteAsync(id, accountId.Value, cancellationToken);
            return Ok(new ApiResponse(ResponseType.Deleted));
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

    [HttpPatch("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var accountId = GetAccountId();
            if (accountId == null) return Unauthorized(new ApiResponse(ResponseType.Unauthorized));

            var address = await _addressService.SetDefaultAsync(id, accountId.Value, cancellationToken);
            return Ok(new ApiResponse(address));
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
