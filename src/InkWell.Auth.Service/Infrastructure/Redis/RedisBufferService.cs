using System.Text.Json;
using InkWell.Auth.Service.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace InkWell.Auth.Service.Infrastructure.Redis;

public class RedisBufferService : IRedisBufferService
{
    private readonly IRedisConnectionService _connectionService;
    private readonly RedisOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisBufferService(
        IRedisConnectionService connectionService,
        IOptions<RedisOptions> options)
    {
        _connectionService = connectionService;
        _options = options.Value;
    }

    public async Task EnqueueAsync(BufferedAuthOperation operation)
    {
        var db = _connectionService.GetDatabase();
        var payload = JsonSerializer.Serialize(operation, JsonOptions);
        await db.ListRightPushAsync(_options.BufferKey, payload);
    }

    public async Task<IReadOnlyList<BufferedAuthOperation>> GetPendingBatchAsync(int batchSize)
    {
        var db = _connectionService.GetDatabase();
        var values = await db.ListRangeAsync(_options.BufferKey, 0, batchSize - 1);

        var result = new List<BufferedAuthOperation>();

        foreach (var value in values)
        {
            if (value.IsNullOrEmpty)
                continue;

            var item = JsonSerializer.Deserialize<BufferedAuthOperation>(value!, JsonOptions);
            if (item != null)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public async Task ClearAsync()
    {
        var db = _connectionService.GetDatabase();
        await db.KeyDeleteAsync(_options.BufferKey);
    }

    public async Task<long> CountAsync()
    {
        var db = _connectionService.GetDatabase();
        return await db.ListLengthAsync(_options.BufferKey);
    }
}