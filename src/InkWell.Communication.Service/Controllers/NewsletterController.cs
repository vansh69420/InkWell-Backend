using System.Security.Claims;
using InkWell.Communication.Service.DTOs.Requests;
using InkWell.Communication.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InkWell.Communication.Service.Controllers;

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _service;

    public NewsletterController(INewsletterService service)
    {
        _service = service;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var userId))
                request.UserId = userId;

            var result = await _service.SubscribeAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpGet("confirm")]
    public async Task<IActionResult> Confirm([FromQuery] string token)
    {
        var result = await _service.ConfirmAsync(token);
        if (!result) return BadRequest("Invalid or expired token.");

        var frontendUrl = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["Frontend:BaseUrl"]
            ?? "http://localhost:4200";

        return Redirect($"{frontendUrl}/newsletter/confirmed");
    }

    [HttpGet("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] string token)
    {
        var result = await _service.UnsubscribeAsync(token);
        if (!result) return BadRequest("Invalid token.");

        var frontendUrl = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["Frontend:BaseUrl"]
            ?? "http://localhost:4200";

        return Redirect($"{frontendUrl}/newsletter/unsubscribed");
    }

    [Authorize]
    [HttpGet("subscribers")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllSubscribersAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNewsletterRequest request)
    {
        try
        {
            await _service.SendNewsletterAsync(request);
            return Ok(new { message = "Newsletter sent." });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var count = await _service.GetSubscriberCountAsync();
        return Ok(new { count });
    }
}