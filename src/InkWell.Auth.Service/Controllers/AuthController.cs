using System.Security.Claims;
using InkWell.Auth.Service.DTOs.Requests;
using InkWell.Auth.Service.DTOs.Responses;
using InkWell.Auth.Service.Models;
using InkWell.Auth.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InkWell.Auth.Service.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string GoogleStateCookie = "inkwell_google_oauth_state";
    private const string GitHubStateCookie = "inkwell_github_oauth_state";
    private const string ReturnUrlCookie = "inkwell_oauth_return_url";
    private const string AccessTokenCookie = "inkwell_access_token";
    private const string RefreshTokenCookie = "inkwell_refresh_token";

    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.Register(request);
            SetAuthCookies(result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.Login(request);
            SetAuthCookies(result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var user = await GetAuthenticatedUserAsync();

            return Ok(new ProfileResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role.ToString(),
                Provider = user.Provider.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var user = await GetAuthenticatedUserAsync();
            await _authService.UpdateProfile(user.UserId, request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var user = await GetAuthenticatedUserAsync();
            await _authService.ChangePassword(user.UserId, request);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("oauth/google")]
    public IActionResult GoogleStart([FromQuery] string? returnUrl = null)
    {
        var state = GenerateState();
        SetTempCookie(GoogleStateCookie, state, TimeSpan.FromMinutes(10));
        SetTempCookie(ReturnUrlCookie, returnUrl ?? GetFrontendSuccessUrl(), TimeSpan.FromMinutes(10));

        var clientId = _configuration["OAuth:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId missing.");
        var redirectUri = _configuration["OAuth:Google:RedirectUri"] ?? throw new InvalidOperationException("Google RedirectUri missing.");
        var scope = Uri.EscapeDataString("openid email profile");

        var url =
            $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&response_type=code" +
            $"&scope={scope}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&access_type=offline" +
            $"&prompt=consent";

        return Redirect(url);
    }

    [HttpGet("oauth/google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (!ValidateState(GoogleStateCookie, state))
        {
            return Redirect(GetFrontendFailureUrl());
        }

        try
        {
            var result = await _authService.LoginWithGoogleAsync(code);
            SetAuthCookies(result);
            ClearTempCookies();

            return Redirect(GetReturnUrl());
        }
        catch
        {
            ClearTempCookies();
            return Redirect(GetFrontendFailureUrl());
        }
    }

    [HttpGet("oauth/github")]
    public IActionResult GitHubStart([FromQuery] string? returnUrl = null)
    {
        var state = GenerateState();
        SetTempCookie(GitHubStateCookie, state, TimeSpan.FromMinutes(10));
        SetTempCookie(ReturnUrlCookie, returnUrl ?? GetFrontendSuccessUrl(), TimeSpan.FromMinutes(10));

        var clientId = _configuration["OAuth:GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub ClientId missing.");
        var redirectUri = _configuration["OAuth:GitHub:RedirectUri"] ?? throw new InvalidOperationException("GitHub RedirectUri missing.");
        var scope = Uri.EscapeDataString("read:user user:email");

        var url =
            $"https://github.com/login/oauth/authorize?client_id={Uri.EscapeDataString(clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={scope}" +
            $"&state={Uri.EscapeDataString(state)}";

        return Redirect(url);
    }

    [HttpGet("oauth/github/callback")]
    public async Task<IActionResult> GitHubCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (!ValidateState(GitHubStateCookie, state))
        {
            return Redirect(GetFrontendFailureUrl());
        }

        try
        {
            var result = await _authService.LoginWithGitHubAsync(code);
            SetAuthCookies(result);
            ClearTempCookies();

            return Redirect(GetReturnUrl());
        }
        catch
        {
            ClearTempCookies();
            return Redirect(GetFrontendFailureUrl());
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshToken(request.RefreshToken);
            SetAuthCookies(result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _authService.Logout(request.RefreshToken);
            Response.Cookies.Delete(AccessTokenCookie);
            Response.Cookies.Delete(RefreshTokenCookie);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<User> GetAuthenticatedUserAsync()
    {
        var userId = GetCurrentUserId();
        var claimedRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(claimedRole))
        {
            throw new UnauthorizedAccessException("Role claim is missing.");
        }

        var user = await _authService.GetUserById(userId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        if (!string.Equals(user.Role.ToString(), claimedRole, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Role validation failed.");
        }

        return user;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token.");
        }

        return userId;
    }

    private string GenerateState()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private void SetTempCookie(string name, string value, TimeSpan expiration)
    {
        Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.Add(expiration),
            IsEssential = true
        });
    }

    private void SetAuthCookies(AuthResponse result)
    {
        var accessExpiryMinutes = _configuration.GetValue<int?>("Jwt:ExpiryMinutes") ?? 1440;
        var refreshExpiryDays = _configuration.GetValue<int?>("Jwt:RefreshTokenExpiryDays") ?? 7;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            IsEssential = true
        };

        Response.Cookies.Append(AccessTokenCookie, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(accessExpiryMinutes),
            IsEssential = true
        });

        if (!string.IsNullOrWhiteSpace(result.RefreshToken))
        {
            Response.Cookies.Append(RefreshTokenCookie, result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(refreshExpiryDays),
                IsEssential = true
            });
        }
    }

    private bool ValidateState(string cookieName, string state)
    {
        return Request.Cookies.TryGetValue(cookieName, out var storedState) &&
               !string.IsNullOrWhiteSpace(storedState) &&
               storedState == state;
    }

    private void ClearTempCookies()
    {
        Response.Cookies.Delete(GoogleStateCookie);
        Response.Cookies.Delete(GitHubStateCookie);
        Response.Cookies.Delete(ReturnUrlCookie);
    }

    private string GetReturnUrl()
    {
        if (Request.Cookies.TryGetValue(ReturnUrlCookie, out var returnUrl) &&
            !string.IsNullOrWhiteSpace(returnUrl))
        {
            return returnUrl;
        }

        return GetFrontendSuccessUrl();
    }

    private string GetFrontendSuccessUrl()
    {
        return _configuration["Frontend:SuccessUrl"] ?? "http://localhost:4200/";
    }

    private string GetFrontendFailureUrl()
    {
        return _configuration["Frontend:FailureUrl"] ?? "http://localhost:4200/login";
    }
}