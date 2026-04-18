using InkWell.Auth.Service.Models;

namespace InkWell.Auth.Service.Infrastructure.Redis;

public interface IRedisBufferService
{
    Task EnqueueAsync(BufferedAuthOperation operation);
    Task<IReadOnlyList<BufferedAuthOperation>> GetPendingBatchAsync(int batchSize);
    Task ClearAsync();
    Task<long> CountAsync();
}