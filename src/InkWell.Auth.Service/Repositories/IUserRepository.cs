using InkWell.Auth.Service.Enums;
using InkWell.Auth.Service.Models;

namespace InkWell.Auth.Service.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
    Task<User?> FindByUsername(string username);
    Task<User?> FindByUserId(Guid userId);
    Task<bool> ExistsByEmail(string email);
    Task<bool> ExistsByUsername(string username);
    Task<IEnumerable<User>> FindAllByRole(UserRole role);
    Task<IEnumerable<User>> SearchByUsername(string keyword);
    Task DeleteByUserId(Guid userId);
}