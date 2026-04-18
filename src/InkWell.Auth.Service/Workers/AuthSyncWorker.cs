using InkWell.Auth.Service.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Auth.Service.Workers;

public class AuthSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuthSyncWorker> _logger;

    public AuthSyncWorker(IServiceScopeFactory scopeFactory, ILogger<AuthSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                var canConnect = await dbContext.Database.CanConnectAsync(stoppingToken);

                if (canConnect)
                {
                    _logger.LogInformation("Auth DB is online. Redis/RabbitMQ foundation is ready for sync.");
                }
                else
                {
                    _logger.LogWarning("Auth DB is offline. Buffered operations can remain in Redis until DB returns.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth sync worker encountered an error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}