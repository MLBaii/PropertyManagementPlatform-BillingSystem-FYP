using PropertyBill.Api.Dtos;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class AuthService : IAuthService
{
    private readonly IResidentRepository _residentRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IResidentRepository residentRepository, IJwtTokenService jwtTokenService)
    {
        _residentRepository = residentRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var resident = await _residentRepository.GetByEmailAsync(request.Email);
        if (resident is null || !BCrypt.Net.BCrypt.Verify(request.Password, resident.PasswordHash))
        {
            return AuthResult.InvalidCredentials();
        }

        if (!resident.IsActive)
        {
            return AuthResult.AccountDisabled();
        }

        var token = _jwtTokenService.GenerateToken(resident.ResidentId, resident.UnitId, "Resident");

        return AuthResult.Success(new LoginResponse
        {
            Token = token,
            ResidentId = resident.ResidentId,
            FullName = resident.FullName,
            Email = resident.Email,
            UnitId = resident.UnitId,
        });
    }
}
