using System.Text;
using System.Text.Json;
using InkWell.Communication.Service.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InkWell.Communication.Service.Infrastructure;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumer(
        IServiceProvider services,
        IConfiguration config,
        ILogger<RabbitMqConsumer> logger)
    {
        _services = services;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(3000, stoppingToken);

        try
        {
            var amqpUrl = _config["RabbitMq:AmqpUrl"] ?? "amqp://guest:guest@localhost:5672";
            var factory = new ConnectionFactory
            {
                Uri = new Uri(amqpUrl)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "inkwell.comment.added",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation(
                    "RabbitMQ message received: {Message}", message);

                try
                {
                    var payload = JsonSerializer.Deserialize<CommentAddedEvent>(message);
                    if (payload != null)
                    {
                        using var scope = _services.CreateScope();
                        var emailService = scope.ServiceProvider
                            .GetRequiredService<IEmailService>();

                        await emailService.SendAsync(
                            payload.AuthorEmail,
                            payload.AuthorUsername,
                            "New comment on your post",
                            $@"
                                <h3>New comment on your post</h3>
                                <p><b>{payload.CommenterUsername}</b> commented on your post.</p>
                                <p style='background:#f5f5f5;padding:12px;border-radius:6px;'>
                                    {payload.CommentContent}
                                </p>
                            "
                        );
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RabbitMQ message.");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: "inkwell.comment.added",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation(
                "RabbitMQ consumer started on queue: inkwell.comment.added");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RabbitMQ consumer stopped gracefully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "RabbitMQ consumer failed to start. Will not retry.");
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public record CommentAddedEvent(
    string PostId,
    string CommentId,
    string AuthorEmail,
    string AuthorUsername,
    string CommenterUsername,
    string CommentContent
);