using System.Text.RegularExpressions;
using InkWell.Auth.Service.DTOs.Requests;

namespace InkWell.Auth.Service.Validation;

public class AuthInputValidator : IAuthInputValidator
{
    public void ValidateRegister(RegisterRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException("Register request cannot be null.");
        }

        ValidateUsername(request.Username);
        ValidateEmail(request.Email);
        ValidatePassword(request.Password);
        ValidateFullName(request.FullName);
    }

    public void ValidateLogin(LoginRequest request)
    {
        if (request == null)
        {
            throw new ArgumentException("Login request cannot be null.");
        }

        ValidateEmail(request.Email);
        ValidatePassword(request.Password);
    }

    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            !Regex.IsMatch(username.Trim(), AuthValidationRules.UsernamePattern))
        {
            throw new ArgumentException("Username must be 3 to 20 characters and can contain letters, numbers, and underscore only.");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            !Regex.IsMatch(email.Trim(), AuthValidationRules.EmailPattern))
        {
            throw new ArgumentException("Email format is invalid.");
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            !Regex.IsMatch(password, AuthValidationRules.PasswordPattern))
        {
            throw new ArgumentException("Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");
        }
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName) ||
            !Regex.IsMatch(fullName.Trim(), AuthValidationRules.FullNamePattern))
        {
            throw new ArgumentException("Full name format is invalid.");
        }
    }
}