using System.Security.Claims;
using InkWell.Comment.Service.DTOs.Requests;
using InkWell.Comment.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace InkWell.Comment.Service.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentController(ICommentService service)
    {
        _service = service;
    }

    [HttpGet("post/{postId}")]
    public async Task<IActionResult> GetByPost(Guid postId)
    {
        var currentUserId = GetCurrentUserId();
        var comments = await _service.GetByPostAsync(postId, currentUserId);
        return Ok(comments);
    }

    [HttpGet("count/{postId}")]
    public async Task<IActionResult> GetCount(Guid postId)
    {
        var count = await _service.GetCommentCountAsync(postId);
        return Ok(new { count });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] AddCommentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Not authenticated.");

            var username = GetClaim(JwtRegisteredClaimNames.UniqueName)
                ?? GetClaim(ClaimTypes.Name)
                ?? "unknown";
            var fullName = username;

            var result = await _service.AddCommentAsync(
                request, userId.Value, username, fullName);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid commentId,
        [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Not authenticated.");

            var result = await _service.UpdateCommentAsync(
                commentId, request, userId.Value);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid commentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Not authenticated.");

            var isAdmin = User.IsInRole("Admin");
            await _service.DeleteCommentAsync(commentId, userId.Value, isAdmin);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
    }

    [HttpPost("{commentId}/like")]
    [Authorize]
    public async Task<IActionResult> Like(Guid commentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Not authenticated.");

            var result = await _service.LikeCommentAsync(commentId, userId.Value);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
    }

    [HttpPost("{commentId}/unlike")]
    [Authorize]
    public async Task<IActionResult> Unlike(Guid commentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Not authenticated.");

            var result = await _service.UnlikeCommentAsync(commentId, userId.Value);
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
    }

    private Guid? GetCurrentUserId()
    {
       var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private string? GetClaim(string type)
    {
        return User.FindFirstValue(type);
    }
}