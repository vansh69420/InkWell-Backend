namespace InkWell.Auth.Service.Models;

public class AuditLog
{
    public Guid AuditLogId { get; set; } = Guid.NewGuid();
    public Guid ActorId { get; set; }
    public string ActorUsername { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}