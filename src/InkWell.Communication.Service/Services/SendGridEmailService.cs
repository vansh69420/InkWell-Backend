using SendGrid;
using SendGrid.Helpers.Mail;

namespace InkWell.Communication.Service.Services;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IConfiguration config,
        ILogger<SendGridEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent)
    {
        var apiKey = _config["SendGrid:ApiKey"]
            ?? throw new InvalidOperationException("SendGrid API key missing.");
        var fromEmail = _config["SendGrid:FromEmail"] ?? "noreply@inkwell.com";
        var fromName = _config["SendGrid:FromName"] ?? "InkWell";

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail, toName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync();
            _logger.LogError("SendGrid error: {StatusCode} {Body}",
                response.StatusCode, body);
        }
        else
        {
            _logger.LogInformation("Email sent to {Email}", toEmail);
        }
    }

    public async Task SendBulkAsync(
        List<string> toEmails,
        string subject,
        string htmlContent)
    {
        var tasks = toEmails.Select(email =>
            SendAsync(email, email, subject, htmlContent));
        await Task.WhenAll(tasks);
    }
}