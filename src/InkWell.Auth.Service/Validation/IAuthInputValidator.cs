using InkWell.Auth.Service.DTOs.Requests;

namespace InkWell.Auth.Service.Validation;

public interface IAuthInputValidator
{
    void ValidateRegister(RegisterRequest request);
    void ValidateLogin(LoginRequest request);
}