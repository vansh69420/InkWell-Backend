using InkWell.Communication.Service.DTOs.Requests;
using InkWell.Communication.Service.DTOs.Responses;
using InkWell.Communication.Service.Models;
using InkWell.Communication.Service.Repositories;

namespace InkWell.Communication.Service.Services;

public class NewsletterServiceImpl : INewsletterService
{
    private readonly ISubscriberRepository _repo;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly ILogger<NewsletterServiceImpl> _logger;

    public NewsletterServiceImpl(
        ISubscriberRepository repo,
        IEmailService emailService,
        IConfiguration config,
        ILogger<NewsletterServiceImpl> logger)
    {
        _repo = repo;
        _emailService = emailService;
        _config = config;
        _logger = logger;
    }

    public async Task<SubscriberResponse> SubscribeAsync(SubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required.");

        var email = request.Email.Trim().ToLower();

        var existing = await _repo.GetByEmailAsync(email);

        if (existing != null)
        {
            if (existing.Status == "Active")
                throw new InvalidOperationException("Already subscribed.");

            if (existing.Status == "Pending")
            {
                await SendConfirmationEmailAsync(existing);
                return Map(existing);
            }

            existing.Status = "Pending";
            existing.Token = Guid.NewGuid().ToString("N");
            existing.SubscribedAt = DateTime.UtcNow;
            existing.UnsubscribedAt = null;
            await _repo.SaveChangesAsync();
            await SendConfirmationEmailAsync(existing);
            return Map(existing);
        }

        var subscriber = new Subscriber
        {
            SubscriberId = Guid.NewGuid(),
            Email = email,
            FullName = request.FullName?.Trim(),
            UserId = request.UserId,
            Status = "Pending",
            Token = Guid.NewGuid().ToString("N"),
            SubscribedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(subscriber);
        await _repo.SaveChangesAsync();

        await SendConfirmationEmailAsync(subscriber);

        return Map(subscriber);
    }

    public async Task<bool> ConfirmAsync(string token)
    {
        var subscriber = await _repo.GetByTokenAsync(token);
        if (subscriber == null) return false;

        if (subscriber.Status == "Active") return true;

        subscriber.Status = "Active";
        subscriber.ConfirmedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync();

        await SendWelcomeEmailAsync(subscriber);

        return true;
    }

    public async Task<bool> UnsubscribeAsync(string token)
    {
        var subscriber = await _repo.GetByTokenAsync(token);
        if (subscriber == null) return false;

        subscriber.Status = "Unsubscribed";
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<List<SubscriberResponse>> GetAllSubscribersAsync()
    {
        var subscribers = await _repo.GetAllAsync();
        return subscribers.Select(Map).ToList();
    }

    public async Task SendNewsletterAsync(SendNewsletterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject is required.");

        if (string.IsNullOrWhiteSpace(request.HtmlContent))
            throw new ArgumentException("Content is required.");

        var activeSubscribers = await _repo.GetActiveAsync();

        if (activeSubscribers.Count == 0)
        {
            _logger.LogInformation("No active subscribers to send newsletter to.");
            return;
        }

        var gatewayUrl = _config["Gateway:BaseUrl"] ?? "http://localhost:5000";

        var tasks = activeSubscribers.Select(subscriber =>
        {
            var unsubscribeUrl = $"{gatewayUrl}/newsletter/unsubscribe?token={subscriber.Token}";
            var htmlWithFooter = $@"
                {request.HtmlContent}
                <hr style='margin-top:40px;border:none;border-top:1px solid #eee;'/>
                <p style='font-size:12px;color:#999;text-align:center;'>
                    You are receiving this because you subscribed to InkWell newsletter.<br/>
                    <a href='{unsubscribeUrl}' style='color:#999;'>Unsubscribe</a>
                </p>
            ";
            return _emailService.SendAsync(
                subscriber.Email,
                subscriber.FullName ?? subscriber.Email,
                request.Subject,
                htmlWithFooter);
        });

        await Task.WhenAll(tasks);

        _logger.LogInformation("Newsletter sent to {Count} subscribers.", activeSubscribers.Count);
    }

    public async Task<int> GetSubscriberCountAsync()
    {
        var all = await _repo.GetAllAsync();
        return all.Count(s => s.Status == "Active");
    }

    private async Task SendConfirmationEmailAsync(Subscriber subscriber)
    {
        var gatewayUrl = _config["Gateway:BaseUrl"] ?? "http://localhost:5000";
        var confirmUrl = $"{gatewayUrl}/newsletter/confirm?token={subscriber.Token}";
        var html = $@"
            <h2>Confirm your subscription to InkWell</h2>
            <p>Click the button below to confirm your subscription:</p>
            <a href='{confirmUrl}' style='background:#7c5cff;color:white;padding:12px 24px;border-radius:8px;text-decoration:none;'>
                Confirm Subscription
            </a>
            <p>If you did not subscribe, ignore this email.</p>
        ";

        await _emailService.SendAsync(
            subscriber.Email,
            subscriber.FullName ?? subscriber.Email,
            "Confirm your InkWell subscription",
            html);
    }

    private async Task SendWelcomeEmailAsync(Subscriber subscriber)
    {
        var gatewayUrl = _config["Gateway:BaseUrl"] ?? "http://localhost:5000";
        var unsubscribeUrl = $"{gatewayUrl}/newsletter/unsubscribe?token={subscriber.Token}";

        var html = $@"
            <h2>Welcome to InkWell! 🎉</h2>
            <p>You are now subscribed to InkWell newsletter.</p>
            <p>You will receive updates when new posts are published.</p>
            <hr/>
            <p style='font-size:12px;color:#999;'>
                <a href='{unsubscribeUrl}'>Unsubscribe</a>
            </p>
        ";

        await _emailService.SendAsync(
            subscriber.Email,
            subscriber.FullName ?? subscriber.Email,
            "Welcome to InkWell!",
            html);
    }

    private SubscriberResponse Map(Subscriber s) => new()
    {
        SubscriberId = s.SubscriberId,
        Email = s.Email,
        FullName = s.FullName,
        UserId = s.UserId,
        Status = s.Status,
        SubscribedAt = s.SubscribedAt,
        ConfirmedAt = s.ConfirmedAt
    };
}