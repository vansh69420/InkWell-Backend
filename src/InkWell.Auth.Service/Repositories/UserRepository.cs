using InkWell.Auth.Service.DbContexts;
using InkWell.Auth.Service.Enums;
using InkWell.Auth.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Auth.Service.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _dbContext;

    public UserRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> FindByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> FindByUsername(string username)
    {
        var normalizedUsername = username.Trim().ToLower();
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Username.ToLower() == normalizedUsername);
    }

    public async Task<User?> FindByUserId(Guid userId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _dbContext.Users.AnyAsync(x => x.Email.ToLower() == normalizedEmail);
    }

    public async Task<bool> ExistsByUsername(string username)
    {
        var normalizedUsername = username.Trim().ToLower();
        return await _dbContext.Users.AnyAsync(x => x.Username.ToLower() == normalizedUsername);
    }

    public async Task<IEnumerable<User>> FindAllByRole(UserRole role)
    {
        return await _dbContext.Users
            .Where(x => x.Role == role)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> SearchByUsername(string keyword)
    {
        var normalizedKeyword = keyword.Trim().ToLower();
        return await _dbContext.Users
            .Where(x => x.Username.ToLower().Contains(normalizedKeyword))
            .ToListAsync();
    }

    public async Task DeleteByUserId(Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        if (user != null)
        {
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}