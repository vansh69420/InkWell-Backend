using System.Text.Json.Serialization;

namespace InkWell.Auth.Service.Models;

public class BufferedAuthOperation
{
    public Guid OperationId { get; set; } = Guid.NewGuid();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BufferedAuthOperationType OperationType { get; set; }

    public string PayloadJson { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}