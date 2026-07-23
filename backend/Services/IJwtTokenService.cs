namespace PropertyBill.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(int residentId, int unitId, string role);
    string GenerateAdminToken(int adminUserId, string role);
}
