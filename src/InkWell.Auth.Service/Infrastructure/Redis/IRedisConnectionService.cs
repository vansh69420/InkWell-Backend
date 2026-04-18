using StackExchange.Redis;

namespace InkWell.Auth.Service.Infrastructure.Redis;

public interface IRedisConnectionService
{
    IDatabase GetDatabase();
    bool IsConnected();
}