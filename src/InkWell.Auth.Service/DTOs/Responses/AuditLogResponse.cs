namespace InkWell.Auth.Service.DTOs.Responses;

public class AuditLogResponse
{
    public Guid AuditLogId { get; set; }
    public Guid ActorId { get; set; }
    public string ActorUsername { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}