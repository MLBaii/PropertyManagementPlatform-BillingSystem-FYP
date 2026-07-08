using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
}
