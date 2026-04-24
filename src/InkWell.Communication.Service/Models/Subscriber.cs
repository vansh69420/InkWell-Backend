namespace InkWell.Communication.Service.Models;

public class Subscriber
{
    public Guid SubscriberId { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public Guid? UserId { get; set; }
    public string Status { get; set; } = "Pending";
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
}