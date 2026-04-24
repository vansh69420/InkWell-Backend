namespace InkWell.Communication.Service.Services;

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlContent);
    Task SendBulkAsync(List<string> toEmails, string subject, string htmlContent);
}