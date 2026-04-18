using InkWell.Auth.Service.DbContexts;
using InkWell.Auth.Service.Enums;
using InkWell.Auth.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Auth.Service.Repositories;

public class ExternalLoginRepository : IExternalLoginRepository
{
    private readonly AuthDbContext _dbContext;

    public ExternalLoginRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExternalLogin?> FindByProviderAndProviderUserIdAsync(AuthProvider provider, string providerUserId)
    {
        return await _dbContext.ExternalLogins
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId);
    }

    public async Task<ExternalLogin?> FindByUserIdAndProviderAsync(Guid userId, AuthProvider provider)
    {
        return await _dbContext.ExternalLogins
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider);
    }

    public async Task AddAsync(ExternalLogin externalLogin)
    {
        await _dbContext.ExternalLogins.AddAsync(externalLogin);
    }

    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}