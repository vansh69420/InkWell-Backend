namespace InkWell.Auth.Service.Infrastructure.RabbitMq;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string QueueName { get; set; } = "inkwell.auth.queue";
    public string ExchangeName { get; set; } = "inkwell.auth.exchange";
}