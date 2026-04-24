using InkWell.Communication.Service.Models;

namespace InkWell.Communication.Service.Repositories;

public interface ISubscriberRepository
{
    Task<Subscriber?> GetByEmailAsync(string email);
    Task<Subscriber?> GetByTokenAsync(string token);
    Task<List<Subscriber>> GetAllAsync();
    Task<List<Subscriber>> GetActiveAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(Subscriber subscriber);
    Task SaveChangesAsync();
}