using InkWell.Auth.Service.Enums;
using InkWell.Auth.Service.Models;

namespace InkWell.Auth.Service.Repositories;

public interface IExternalLoginRepository
{
    Task<ExternalLogin?> FindByProviderAndProviderUserIdAsync(AuthProvider provider, string providerUserId);
    Task<ExternalLogin?> FindByUserIdAndProviderAsync(Guid userId, AuthProvider provider);
    Task AddAsync(ExternalLogin externalLogin);
    Task SaveChangesAsync();
}