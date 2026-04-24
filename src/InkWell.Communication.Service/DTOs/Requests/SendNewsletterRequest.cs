namespace InkWell.Communication.Service.DTOs.Requests;

public class SendNewsletterRequest
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
}