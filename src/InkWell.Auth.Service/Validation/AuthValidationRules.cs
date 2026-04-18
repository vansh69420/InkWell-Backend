namespace InkWell.Auth.Service.Validation;

public static class AuthValidationRules
{
    public const string UsernamePattern = @"^[a-zA-Z0-9_]{3,20}$";
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$";
    public const string FullNamePattern = @"^[A-Za-z][A-Za-z\s.'-]{1,98}[A-Za-z]$";
}