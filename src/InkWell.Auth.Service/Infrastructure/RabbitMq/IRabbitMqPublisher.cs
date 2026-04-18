namespace InkWell.Auth.Service.Infrastructure.RabbitMq;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string routingKey, string message, CancellationToken cancellationToken = default);
    Task PublishJsonAsync<T>(string routingKey, T payload, CancellationToken cancellationToken = default);
}