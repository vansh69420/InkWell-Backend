using InkWell.Communication.Service.DbContexts;
using InkWell.Communication.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Communication.Service.Repositories;

public class SubscriberRepository : ISubscriberRepository
{
    private readonly CommunicationDbContext _db;

    public SubscriberRepository(CommunicationDbContext db)
    {
        _db = db;
    }

    public async Task<Subscriber?> GetByEmailAsync(string email)
    {
        return await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email == email.ToLower());
    }

    public async Task<Subscriber?> GetByTokenAsync(string token)
    {
        return await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Token == token);
    }

    public async Task<List<Subscriber>> GetAllAsync()
    {
        return await _db.Subscribers
            .OrderByDescending(s => s.SubscribedAt)
            .ToListAsync();
    }

    public async Task<List<Subscriber>> GetActiveAsync()
    {
        return await _db.Subscribers
            .Where(s => s.Status == "Active")
            .ToListAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _db.Subscribers
            .AnyAsync(s => s.Email == email.ToLower());
    }

    public async Task AddAsync(Subscriber subscriber)
    {
        await _db.Subscribers.AddAsync(subscriber);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}