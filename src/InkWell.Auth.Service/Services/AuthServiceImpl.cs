using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using InkWell.Auth.Service.DbContexts;
using InkWell.Auth.Service.DTOs.Requests;
using InkWell.Auth.Service.DTOs.Responses;
using InkWell.Auth.Service.Enums;
using InkWell.Auth.Service.Models;
using InkWell.Auth.Service.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using InkWell.Auth.Service.Validation;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace InkWell.Auth.Service.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly AuthDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IAuthInputValidator _authInputValidator;
    private readonly IExternalLoginRepository _externalLoginRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthServiceImpl(
    AuthDbContext dbContext,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IExternalLoginRepository externalLoginRepository,
    IAuthInputValidator authInputValidator,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration)
{
    _dbContext = dbContext;
    _userRepository = userRepository;
    _refreshTokenRepository = refreshTokenRepository;
    _externalLoginRepository = externalLoginRepository;
    _authInputValidator = authInputValidator;
    _httpClientFactory = httpClientFactory;
    _configuration = configuration;
}

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        _authInputValidator.ValidateRegister(request);
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("All fields are required.");
        }

        if (await _userRepository.ExistsByEmail(request.Email))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        if (await _userRepository.ExistsByUsername(request.Username))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Reader,
            Provider = AuthProvider.Local,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        _authInputValidator.ValidateLogin(request);
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var user = await _userRepository.FindByEmail(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is inactive.");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await IssueTokensAsync(user);
    }

    public async Task Logout(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenHash = ComputeTokenHash(refreshToken);
        var storedToken = await _refreshTokenRepository.FindByTokenHash(tokenHash);

        if (storedToken == null)
        {
            return;
        }

        await _refreshTokenRepository.RevokeTokenAsync(storedToken);
        await _refreshTokenRepository.SaveChangesAsync();
    }

    public Task<bool> ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                return Task.FromResult(false);
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            handler.ValidateToken(token, validationParameters, out _);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<AuthResponse> RefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var tokenHash = ComputeTokenHash(refreshToken);
        var storedToken = await _refreshTokenRepository.FindByTokenHash(tokenHash);

        if (storedToken == null ||
            storedToken.IsRevoked ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await _userRepository.FindByUserId(storedToken.UserId);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var (newRawRefreshToken, newRefreshTokenEntity) = CreateRefreshToken(user.UserId);

        await _refreshTokenRepository.RevokeTokenAsync(storedToken, newRefreshTokenEntity.TokenHash);
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            AccessToken = GenerateAccessToken(user),
            RefreshToken = newRawRefreshToken
        };
    }

    public Task<User?> GetUserByEmail(string email)
    {
        return _userRepository.FindByEmail(email);
    }

    public Task<User?> GetUserById(Guid userId)
    {
        return _userRepository.FindByUserId(userId);
    }

    public Task UpdateProfile(UpdateProfileRequest request)
    {
        throw new NotImplementedException("Profile update will be implemented in UC2 Task 5.");
    }

    public Task ChangePassword(ChangePasswordRequest request)
    {
        throw new NotImplementedException("Change password will be implemented in UC2 Task 5.");
    }

    public Task<IEnumerable<User>> SearchUsers(string keyword)
    {
        throw new NotImplementedException("User search will be implemented later.");
    }

    public Task DeactivateAccount(Guid userId)
    {
        throw new NotImplementedException("Account deactivation will be implemented later.");
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var (rawRefreshToken, refreshTokenEntity) = CreateRefreshToken(user.UserId);

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            AccessToken = GenerateAccessToken(user),
            RefreshToken = rawRefreshToken
        };
    }

    private (string RawToken, RefreshToken Entity) CreateRefreshToken(Guid userId)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = ComputeTokenHash(rawToken);

        var refreshTokenExpiryDays = 7;
        var configuredValue = _configuration["Jwt:RefreshTokenExpiryDays"];

        if (int.TryParse(configuredValue, out var parsedDays) && parsedDays > 0)
        {
            refreshTokenExpiryDays = parsedDays;
        }

        var entity = new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        return (rawToken, entity);
    }

    private static string ComputeTokenHash(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }

    private string GenerateAccessToken(User user)
    {
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutesValue = _configuration["Jwt:ExpiryMinutes"];

        if (string.IsNullOrWhiteSpace(key) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JWT settings are not configured.");
        }

        var expiryMinutes = 1440;

        if (int.TryParse(expiryMinutesValue, out var configuredExpiryMinutes))
        {
            expiryMinutes = configuredExpiryMinutes;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(string code)
{
    var profile = await ExchangeGoogleCodeAsync(code);
    return await CreateOrLinkOAuthUserAsync(AuthProvider.Google, profile);
}

public async Task<AuthResponse> LoginWithGitHubAsync(string code)
{
    var profile = await ExchangeGitHubCodeAsync(code);
    return await CreateOrLinkOAuthUserAsync(AuthProvider.GitHub, profile);
}

private async Task<AuthResponse> CreateOrLinkOAuthUserAsync(AuthProvider provider, OAuthProfile profile)
{
    var externalLogin = await _externalLoginRepository
        .FindByProviderAndProviderUserIdAsync(provider, profile.ProviderUserId);

    User? user = null;

    if (externalLogin != null)
    {
        user = await _userRepository.FindByUserId(externalLogin.UserId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Linked OAuth user was not found.");
        }

        externalLogin.LastLoginAt = DateTime.UtcNow;
        await _externalLoginRepository.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    user = await _userRepository.FindByEmail(profile.Email);

    if (user == null)
    {
        var username = await GenerateUniqueUsernameAsync(profile.Username);

        user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = profile.Email.Trim(),
            FullName = profile.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
            Role = UserRole.Reader,
            Provider = provider,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Users.AddAsync(user);
    }

    var existingLink = await _externalLoginRepository.FindByUserIdAndProviderAsync(user.UserId, provider);

    if (existingLink == null)
    {
        await _externalLoginRepository.AddAsync(new ExternalLogin
        {
            ExternalLoginId = Guid.NewGuid(),
            UserId = user.UserId,
            Provider = provider,
            ProviderUserId = profile.ProviderUserId,
            Email = profile.Email.Trim(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        });
    }
    else
    {
        existingLink.LastLoginAt = DateTime.UtcNow;
    }

    await _dbContext.SaveChangesAsync();

    return await IssueTokensAsync(user);
}

private async Task<OAuthProfile> ExchangeGoogleCodeAsync(string code)
{
    var clientId = _configuration["OAuth:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is missing.");
    var clientSecret = _configuration["OAuth:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret is missing.");
    var redirectUri = _configuration["OAuth:Google:RedirectUri"] ?? throw new InvalidOperationException("Google RedirectUri is missing.");

    var http = _httpClientFactory.CreateClient();

    var tokenResponse = await http.PostAsync(
        "https://oauth2.googleapis.com/token",
        new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        }));

    tokenResponse.EnsureSuccessStatusCode();

    var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
    var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();

    if (string.IsNullOrWhiteSpace(accessToken))
    {
        throw new UnauthorizedAccessException("Google access token was not returned.");
    }

    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var userResponse = await http.GetAsync("https://openidconnect.googleapis.com/v1/userinfo");
    userResponse.EnsureSuccessStatusCode();

    var userJson = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());

    var providerUserId = userJson.RootElement.GetProperty("sub").GetString() ?? throw new UnauthorizedAccessException("Google user id missing.");
    var email = userJson.RootElement.GetProperty("email").GetString() ?? throw new UnauthorizedAccessException("Google email missing.");
    var fullName = userJson.RootElement.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? email : email;
    var username = email.Split('@')[0];

    return new OAuthProfile(providerUserId, email, fullName ?? email, username);
}

private async Task<OAuthProfile> ExchangeGitHubCodeAsync(string code)
{
    var clientId = _configuration["OAuth:GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub ClientId is missing.");
    var clientSecret = _configuration["OAuth:GitHub:ClientSecret"] ?? throw new InvalidOperationException("GitHub ClientSecret is missing.");
    var redirectUri = _configuration["OAuth:GitHub:RedirectUri"] ?? throw new InvalidOperationException("GitHub RedirectUri is missing.");

    var http = _httpClientFactory.CreateClient();
    http.DefaultRequestHeaders.UserAgent.ParseAdd("InkWell/1.0");

    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
    tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["code"] = code,
        ["client_id"] = clientId,
        ["client_secret"] = clientSecret,
        ["redirect_uri"] = redirectUri
    });

    var tokenResponse = await http.SendAsync(tokenRequest);
    tokenResponse.EnsureSuccessStatusCode();

    var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
    var accessToken = tokenJson.RootElement.GetProperty("access_token").GetString();

    if (string.IsNullOrWhiteSpace(accessToken))
    {
        throw new UnauthorizedAccessException("GitHub access token was not returned.");
    }

    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var userResponse = await http.GetAsync("https://api.github.com/user");
    userResponse.EnsureSuccessStatusCode();

    var userJson = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync());

    var providerUserId = userJson.RootElement.GetProperty("id").GetRawText();
    var fullName = userJson.RootElement.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
    var login = userJson.RootElement.GetProperty("login").GetString() ?? "github-user";
    var email = userJson.RootElement.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;

    if (string.IsNullOrWhiteSpace(email))
    {
        var emailsResponse = await http.GetAsync("https://api.github.com/user/emails");
        emailsResponse.EnsureSuccessStatusCode();

        var emailsJson = JsonDocument.Parse(await emailsResponse.Content.ReadAsStringAsync());
        foreach (var item in emailsJson.RootElement.EnumerateArray())
        {
            var isPrimary = item.TryGetProperty("primary", out var primaryEl) && primaryEl.GetBoolean();
            var isVerified = item.TryGetProperty("verified", out var verifiedEl) && verifiedEl.GetBoolean();

            if (isPrimary && isVerified && item.TryGetProperty("email", out var selectedEmail))
            {
                email = selectedEmail.GetString();
                break;
            }
        }
    }

    if (string.IsNullOrWhiteSpace(email))
    {
        throw new UnauthorizedAccessException("GitHub email missing.");
    }

    return new OAuthProfile(providerUserId, email, fullName ?? login, login);
}

private async Task<string> GenerateUniqueUsernameAsync(string baseUsername)
{
    var normalized = Regex.Replace(baseUsername.ToLowerInvariant(), @"[^a-z0-9_]", "");
    if (string.IsNullOrWhiteSpace(normalized))
    {
        normalized = "user";
    }

    var candidate = normalized;
    var index = 1;

    while (await _userRepository.ExistsByUsername(candidate))
    {
        candidate = $"{normalized}{index}";
        index++;
    }

    return candidate;
}

private sealed record OAuthProfile(string ProviderUserId, string Email, string FullName, string Username);

public async Task UpdateProfile(Guid userId, UpdateProfileRequest request)
{
    if (request == null)
    {
        throw new ArgumentNullException(nameof(request));
    }

    if (string.IsNullOrWhiteSpace(request.FullName))
    {
        throw new ArgumentException("Full name is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        throw new ArgumentException("Email is required.");
    }

    var user = await _userRepository.FindByUserId(userId);

    if (user == null)
    {
        throw new UnauthorizedAccessException("User not found.");
    }

    if (!user.IsActive)
    {
        throw new UnauthorizedAccessException("Account is inactive.");
    }

    var normalizedEmail = request.Email.Trim();

    if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
        await _userRepository.ExistsByEmail(normalizedEmail))
    {
        throw new InvalidOperationException("Email already exists.");
    }

    user.FullName = request.FullName.Trim();
    user.Email = normalizedEmail;
    user.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
    user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

    await _dbContext.SaveChangesAsync();
}

public async Task ChangePassword(Guid userId, ChangePasswordRequest request)
{
    if (request == null)
    {
        throw new ArgumentNullException(nameof(request));
    }

    if (string.IsNullOrWhiteSpace(request.NewPassword))
    {
        throw new ArgumentException("New password is required.");
    }

    var user = await _userRepository.FindByUserId(userId);

    if (user == null)
    {
        throw new UnauthorizedAccessException("User not found.");
    }

    if (!user.IsActive)
    {
        throw new UnauthorizedAccessException("Account is inactive.");
    }

    // Local accounts must verify the current password.
    // OAuth users can set a local password after being authenticated in-session.
    if (user.Provider == AuthProvider.Local)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            throw new ArgumentException("Current password is required.");
        }

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!isCurrentPasswordValid)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

    await _dbContext.SaveChangesAsync();
}
}