using Microsoft.AspNetCore.Mvc;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Route("api/auth/admin")]
public class AdminAuthController(IAdminAuthService adminAuthService) : ControllerBase { [HttpPost("login")] public async Task<ActionResult<AdminLoginResponse>> Login([FromBody] AdminLoginRequest request) { var response = await adminAuthService.LoginAsync(request); return response is null ? Unauthorized(new { message = "Invalid username or password." }) : Ok(response); } }
