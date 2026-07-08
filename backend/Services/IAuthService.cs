using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IAuthService
{
    LoginResponse Login(LoginRequest request);
}
