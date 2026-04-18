using InkWell.Auth.Service.DTOs.Requests;
using InkWell.Auth.Service.DTOs.Responses;
using InkWell.Auth.Service.Models;

namespace InkWell.Auth.Service.Services;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterRequest request);
    Task<AuthResponse> Login(LoginRequest request);
    Task Logout(string refreshToken);
    Task<bool> ValidateToken(string token);
    Task<AuthResponse> RefreshToken(string refreshToken);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserById(Guid userId);
    Task UpdateProfile(Guid userId, UpdateProfileRequest request);
    Task ChangePassword(Guid userId, ChangePasswordRequest request);
    Task<IEnumerable<User>> SearchUsers(string keyword);
    Task DeactivateAccount(Guid userId);

    Task<AuthResponse> LoginWithGoogleAsync(string code);
    Task<AuthResponse> LoginWithGitHubAsync(string code);
}