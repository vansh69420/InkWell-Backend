using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InkWell.Gateway.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IHttpClientFactory httpClientFactory,
        ILogger<AnalyticsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("platform")]
    public async Task<IActionResult> GetPlatformAnalytics()
    {
        var cookieHeader = Request.Headers["Cookie"].ToString();

        var totalUsers = await FetchCountAsync("auth", "/api/admin/users/count", cookieHeader);
        var totalPosts = await FetchCountAsync("posts", "/api/posts/count", cookieHeader);
        var totalComments = await FetchCountAsync("comments", "/api/comments/count/all", cookieHeader);
        var newsletterCount = await FetchCountAsync("newsletter", "/api/newsletter/count", cookieHeader);
        var mostViewedRaw = await FetchRawStringAsync("posts", "/api/posts/published", cookieHeader);

        var json = $@"{{
            ""totalUsers"": {totalUsers},
            ""totalPosts"": {totalPosts},
            ""totalComments"": {totalComments},
            ""newsletterSubscribers"": {newsletterCount},
            ""mostViewedPosts"": {mostViewedRaw}
        }}";

        return Content(json, "application/json");
    }

    private async Task<int> FetchCountAsync(
        string clientName,
        string path,
        string cookieHeader)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            if (!string.IsNullOrWhiteSpace(cookieHeader))
                request.Headers.Add("Cookie", cookieHeader);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return 0;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("count", out var countEl))
                return countEl.GetInt32();

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch count from {Client}{Path}",
                clientName, path);
            return 0;
        }
    }

    private async Task<string> FetchRawStringAsync(
        string clientName,
        string path,
        string cookieHeader)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            if (!string.IsNullOrWhiteSpace(cookieHeader))
                request.Headers.Add("Cookie", cookieHeader);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return "[]";

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch raw string from {Client}{Path}",
                clientName, path);
            return "[]";
        }
    }
}