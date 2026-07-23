using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Route("api/auth/resident")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(IAuthService authService, IPasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
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

    // UC-101 A1 (forgot password). Always 200 with the same message whether or not the
    // email matches a resident — see PasswordResetService.RequestResetAsync for why.
    [HttpPost("forgot-password")]
    [EnableRateLimiting("PasswordResetRequestPolicy")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _passwordResetService.RequestResetAsync(request.Email);
        return Ok(new { message = "A password reset link has been sent to your email. Please check your inbox." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("PasswordResetVerifyPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _passwordResetService.ResetPasswordAsync(request.Token, request.NewPassword);

        return result.Status switch
        {
            PasswordResetResultStatus.Success => Ok(new
            {
                message = "Your password has been reset successfully. Please log in with your new password.",
            }),
            _ => BadRequest(new
            {
                message = "This reset code is invalid or has expired. Please request a new one.",
            }),
        };
    }
}
