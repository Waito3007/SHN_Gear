using RefreshTokenEntity = SHNGearBE.Models.Entities.Account.RefreshToken;

namespace SHNGearBE.Repositorys.Interface.RefreshToken;

public interface IRefreshTokenRepository : IGenericRepository<RefreshTokenEntity>
{
    Task<RefreshTokenEntity?> GetByTokenAsync(string token);
    Task<RefreshTokenEntity?> GetByJwtTokenAsync(string jwtToken);
    Task RevokeTokenAsync(Guid tokenId);
    Task RevokeAllUserTokensAsync(Guid accountId);
    Task<IEnumerable<RefreshTokenEntity>> GetActiveTokensByAccountAsync(Guid accountId);
}
