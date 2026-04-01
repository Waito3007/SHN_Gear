using BackgroundLogService.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Models.DTOs.Account;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Account;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogService<AuthController> _logService;

    public AuthController(IAuthService authService, ILogService<AuthController> logService)
    {
        _authService = authService;
        _logService = logService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(new ApiResponse(result));
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

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(new ApiResponse(result));
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

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(new ApiResponse(result));
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

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var accountIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return Unauthorized(new ApiResponse(ResponseType.Unauthorized));
            }

            await _authService.LogoutAsync(accountId);
            return Ok(new ApiResponse(new { message = "Logged out successfully" }));
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

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var accountIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return Unauthorized(new ApiResponse(ResponseType.Unauthorized));
            }

            var result = await _authService.ChangePasswordAsync(accountId, request);
            return Ok(new ApiResponse(new { message = "Password changed successfully" }));
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

    [HttpPost("send-verification-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendVerificationOtp([FromBody] SendOtpRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.SendEmailVerificationOtpAsync(request.Email, cancellationToken);
            return Ok(new ApiResponse(new { message = "OTP sent" }, ResponseType.Success));
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

    [HttpPost("verify-email-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyOtpRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _authService.VerifyEmailOtpAsync(request.Email, request.Otp, cancellationToken);
            if (!ok)
            {
                return BadRequest(new ApiResponse(ResponseType.InvalidData));
            }

            return Ok(new ApiResponse(new { message = "Email verified" }, ResponseType.Success));
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

    [HttpPost("forgot-password/send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendForgotPasswordOtp([FromBody] SendOtpRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.SendForgotPasswordOtpAsync(request.Email, cancellationToken);
            return Ok(new ApiResponse(new { message = "If the account exists, OTP has been sent" }, ResponseType.Success));
        }
        catch (ProjectException ex)
        {
            await _logService.WriteMessageAsync($"SendForgotPasswordOtp failed: {ex.Message}");
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPost("forgot-password/verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyOtpRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.VerifyForgotPasswordOtpAsync(request.Email, request.Otp, cancellationToken);
            return Ok(new ApiResponse(result));
        }
        catch (ProjectException ex)
        {
            await _logService.WriteMessageAsync($"VerifyForgotPasswordOtp failed: {ex.Message}");
            return StatusCode(ex.ResponseType.ToHttpStatusCode(), new ApiResponse(ex.ResponseType));
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            return StatusCode(500, new ApiResponse(ResponseType.InternalServerError));
        }
    }

    [HttpPost("forgot-password/reset")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetForgotPassword([FromBody] ResetForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _authService.ResetForgotPasswordAsync(request, cancellationToken);
            if (!ok)
            {
                return BadRequest(new ApiResponse(ResponseType.InvalidData));
            }

            return Ok(new ApiResponse(new { message = "Password reset successful" }, ResponseType.Success));
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
