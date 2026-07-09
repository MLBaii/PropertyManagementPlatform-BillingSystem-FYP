using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IDashboardService
{
    Task<DashboardDto?> GetDashboardAsync(int residentId, int unitId);
}
