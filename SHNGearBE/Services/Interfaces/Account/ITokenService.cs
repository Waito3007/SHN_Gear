using System.Security.Claims;

namespace SHNGearBE.Services.Interfaces.Account;

public interface ITokenService
{
    string GenerateAccessToken(Guid accountId, string email, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Guid? GetAccountIdFromToken(string token);
}
