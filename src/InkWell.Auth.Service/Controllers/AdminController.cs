using System.Security.Claims;
using InkWell.Auth.Service.DbContexts;
using InkWell.Auth.Service.DTOs.Requests;
using InkWell.Auth.Service.DTOs.Responses;
using InkWell.Auth.Service.Enums;
using InkWell.Auth.Service.Models;
using InkWell.Auth.Service.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Auth.Service.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly AuthDbContext _db;

    public AdminController(
        IUserRepository userRepository,
        AuthDbContext db)
    {
        _userRepository = userRepository;
        _db = db;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        if (!IsAdmin()) return Forbid();

        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var response = users.Select(u => new UserListResponse
        {
            UserId = u.UserId,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            Role = u.Role.ToString(),
            Provider = u.Provider.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });

        return Ok(response);
    }

    [HttpGet("users/count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserCount()
    {
        var count = await _db.Users.CountAsync();
        return Ok(new { count });
    }

    [HttpPut("users/{userId}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid userId,
        [FromBody] ChangeRoleRequest request)
    {
        if (!IsAdmin()) return Forbid();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest("Invalid role.");

        var oldRole = user.Role.ToString();
        user.Role = role;
        await _db.SaveChangesAsync();

        await LogAsync("CHANGE_ROLE", "User", userId.ToString(),
            $"Role changed from {oldRole} to {role}");

        return Ok(new UserListResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Provider = user.Provider.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId)
    {
        if (!IsAdmin()) return Forbid();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _db.SaveChangesAsync();

        await LogAsync("SUSPEND_USER", "User", userId.ToString(),
            $"User {user.Username} suspended.");

        return NoContent();
    }

    [HttpPut("users/{userId}/reactivate")]
    public async Task<IActionResult> ReactivateUser(Guid userId)
    {
        if (!IsAdmin()) return Forbid();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.IsActive = true;
        await _db.SaveChangesAsync();

        await LogAsync("REACTIVATE_USER", "User", userId.ToString(),
            $"User {user.Username} reactivated.");

        return NoContent();
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        if (!IsAdmin()) return Forbid();

        var currentUserId = GetCurrentUserId();
        if (currentUserId == userId)
            return BadRequest("Cannot delete your own account.");

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        var username = user.Username;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        await LogAsync("DELETE_USER", "User", userId.ToString(),
            $"User {username} permanently deleted.");

        return NoContent();
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        if (!IsAdmin()) return Forbid();

        var logs = await _db.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(200)
            .Select(l => new AuditLogResponse
            {
                AuditLogId = l.AuditLogId,
                ActorId = l.ActorId,
                ActorUsername = l.ActorUsername,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Details = l.Details,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }

    private async Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        string? details)
    {
        var actorId = GetCurrentUserId();
        var actorUsername = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";

        var log = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ActorId = actorId,
            ActorUsername = actorUsername,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _db.AuditLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}