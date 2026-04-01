using SHNGearBE.Models.DTOs.Account;

namespace SHNGearBE.Services.Interfaces.Account;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task RevokeTokenAsync(string token);
    Task LogoutAsync(Guid accountId);
    Task<bool> ChangePasswordAsync(Guid accountId, ChangePasswordRequestDto request);
    Task SendEmailVerificationOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailOtpAsync(string email, string otp, CancellationToken cancellationToken = default);
    Task SendForgotPasswordOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<VerifyForgotPasswordOtpResponseDto> VerifyForgotPasswordOtpAsync(string email, string otp, CancellationToken cancellationToken = default);
    Task<bool> ResetForgotPasswordAsync(ResetForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
}
