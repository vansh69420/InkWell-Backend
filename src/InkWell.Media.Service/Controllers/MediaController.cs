using System.Security.Claims;
using InkWell.Media.Service.DTOs.Requests;
using InkWell.Media.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InkWell.Media.Service.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _service;

    public MediaController(IMediaService service)
    {
        _service = service;
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.UploadAsync(file, userId.Value);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyMedia()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _service.GetMyMediaAsync(userId.Value);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllMediaAsync();
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{mediaId}/alt-text")]
    public async Task<IActionResult> UpdateAltText(
        Guid mediaId,
        [FromBody] UpdateAltTextRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var result = await _service.UpdateAltTextAsync(
                mediaId, userId.Value, isAdmin, request);

            return result == null ? NotFound() : Ok(result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [Authorize]
    [HttpDelete("{mediaId}")]
    public async Task<IActionResult> Delete(Guid mediaId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var result = await _service.DeleteAsync(mediaId, userId.Value, isAdmin);

            return result ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [Authorize]
    [HttpPut("{mediaId}/link-post")]
    public async Task<IActionResult> LinkToPost(
        Guid mediaId,
        [FromBody] LinkPostRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var result = await _service.LinkToPostAsync(
                mediaId, userId.Value, isAdmin, request);

            return result == null ? NotFound() : Ok(result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}