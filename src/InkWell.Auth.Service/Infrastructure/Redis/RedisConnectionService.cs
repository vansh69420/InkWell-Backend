using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace InkWell.Auth.Service.Infrastructure.Redis;

public class RedisConnectionService : IRedisConnectionService
{
    private readonly RedisOptions _options;
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

    public RedisConnectionService(IOptions<RedisOptions> options)
    {
        _options = options.Value;

        _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var config = ConfigurationOptions.Parse(_options.Configuration);
            config.AbortOnConnectFail = false;
            config.ConnectRetry = 3;
            config.ConnectTimeout = 5000;

            return ConnectionMultiplexer.Connect(config);
        });
    }

    public IDatabase GetDatabase()
    {
        return _lazyConnection.Value.GetDatabase();
    }

    public bool IsConnected()
    {
        return _lazyConnection.IsValueCreated && _lazyConnection.Value.IsConnected;
    }
}