using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace InkWell.Auth.Service.Infrastructure.RabbitMq;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly Lazy<IConnection> _lazyConnection;
    private IModel? _channel;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;

        _lazyConnection = new Lazy<IConnection>(() =>
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            return factory.CreateConnection();
        });
    }

    public Task PublishAsync(string routingKey, string message, CancellationToken cancellationToken = default)
    {
        var channel = GetChannel();

        channel.QueueDeclare(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        channel.BasicPublish(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public Task PublishJsonAsync<T>(string routingKey, T payload, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);
        return PublishAsync(routingKey, json, cancellationToken);
    }

    private IModel GetChannel()
    {
        if (_channel != null && _channel.IsOpen)
        {
            return _channel;
        }

        var connection = _lazyConnection.Value;
        _channel = connection.CreateModel();
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Dispose();

        if (_lazyConnection.IsValueCreated)
        {
            _lazyConnection.Value.Dispose();
        }
    }
}