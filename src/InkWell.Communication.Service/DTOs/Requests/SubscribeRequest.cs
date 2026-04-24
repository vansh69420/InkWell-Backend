namespace InkWell.Communication.Service.DTOs.Requests;

public class SubscribeRequest
{
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public Guid? UserId { get; set; }
}