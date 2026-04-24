namespace InkWell.Communication.Service.DTOs.Responses;

public class SubscriberResponse
{
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public Guid? UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}