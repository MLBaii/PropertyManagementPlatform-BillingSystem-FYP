using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

// Stub: issues a JWT without validating against the Resident table yet.
// Real credential lookup + password verification lands once Repositories/ResidentRepository
// and the migration are wired up against live data.
public class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    public LoginResponse Login(LoginRequest request)
    {
        const int stubResidentId = 1;
        const int stubUnitId = 1;
        const string stubRole = "Resident";

        var token = _jwtTokenService.GenerateToken(stubResidentId, stubUnitId, stubRole);
        return new LoginResponse { Token = token };
    }
}
