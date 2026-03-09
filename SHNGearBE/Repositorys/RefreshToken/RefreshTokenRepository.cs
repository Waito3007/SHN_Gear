using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using RefreshTokenEntity = SHNGearBE.Models.Entities.Account.RefreshToken;
using SHNGearBE.Repositorys.Interface.RefreshToken;

namespace SHNGearBE.Repositorys.RefreshToken;

public class RefreshTokenRepository : GenericRepository<RefreshTokenEntity>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshTokenEntity?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.Account)
            .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDelete);
    }

    public async Task<RefreshTokenEntity?> GetByJwtTokenAsync(string jwtToken)
    {
        return await _dbSet
            .Include(rt => rt.Account)
            .FirstOrDefaultAsync(rt => rt.JwtToken == jwtToken && !rt.IsDelete);
    }

    public async Task RevokeTokenAsync(Guid tokenId)
    {
        var token = await GetByIdAsync(tokenId);
        if (token != null)
        {
            token.IsRevoked = true;
            token.UpdateAt = DateTime.UtcNow;
            await UpdateAsync(token);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid accountId)
    {
        var tokens = await _dbSet
            .Where(rt => rt.AccountId == accountId && !rt.IsRevoked && !rt.IsDelete)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.UpdateAt = DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<RefreshTokenEntity>> GetActiveTokensByAccountAsync(Guid accountId)
    {
        return await _dbSet
            .Where(rt => rt.AccountId == accountId
                && !rt.IsRevoked
                && !rt.IsUsed
                && rt.Expires > DateTime.UtcNow
                && !rt.IsDelete)
            .ToListAsync();
    }
}
