using InkWell.Auth.Service.Models;

namespace InkWell.Auth.Service.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenHash(string tokenHash);
    Task<IEnumerable<RefreshToken>> FindActiveByUserId(Guid userId);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeTokenAsync(RefreshToken refreshToken, string? replacedByTokenHash = null);
    Task RevokeAllByUserId(Guid userId);
    Task SaveChangesAsync();
}