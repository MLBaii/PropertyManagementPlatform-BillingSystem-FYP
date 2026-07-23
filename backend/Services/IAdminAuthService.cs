using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Services;
public interface IAdminAuthService { Task<AdminLoginResponse?> LoginAsync(AdminLoginRequest request); }
