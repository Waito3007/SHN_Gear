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
}
