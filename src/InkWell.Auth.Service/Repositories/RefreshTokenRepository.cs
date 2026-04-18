using InkWell.Auth.Service.DbContexts;
using InkWell.Auth.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Auth.Service.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _dbContext;

    public RefreshTokenRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken?> FindByTokenHash(string tokenHash)
    {
        return await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task<IEnumerable<RefreshToken>> FindActiveByUserId(Guid userId)
    {
        return await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
    }

    public Task RevokeTokenAsync(RefreshToken refreshToken, string? replacedByTokenHash = null)
    {
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByTokenHash = replacedByTokenHash;

        _dbContext.RefreshTokens.Update(refreshToken);
        return Task.CompletedTask;
    }

    public async Task RevokeAllByUserId(Guid userId)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            _dbContext.RefreshTokens.Update(token);
        }
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}