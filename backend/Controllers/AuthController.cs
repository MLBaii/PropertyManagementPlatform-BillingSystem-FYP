using Microsoft.AspNetCore.Mvc;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Route("api/auth/resident")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return result.Status switch
        {
            AuthResultStatus.Success => Ok(result.Response),
            AuthResultStatus.AccountDisabled => StatusCode(StatusCodes.Status403Forbidden,
                new { message = "This resident account is disabled." }),
            _ => Unauthorized(new { message = "Invalid email or password." }),
        };
    }
}
