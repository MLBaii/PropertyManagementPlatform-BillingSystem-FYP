using PropertyBill.Api.Dtos;
using PropertyBill.Api.Repositories;
namespace PropertyBill.Api.Services;
public class AdminAuthService(IAdminUserRepository adminUserRepository, IJwtTokenService jwtTokenService) : IAdminAuthService { public async Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request) { var admin = await adminUserRepository.GetByUsernameAsync(request.Username.Trim()); if (admin is null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash)) return null; return new AdminLoginResponse { Token = jwtTokenService.GenerateAdminToken(admin.AdminUserId, admin.Role), AdminUserId = admin.AdminUserId, Username = admin.Username, Email = admin.Email, Role = admin.Role }; } }
