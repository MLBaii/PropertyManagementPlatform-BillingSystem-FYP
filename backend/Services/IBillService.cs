using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IBillService
{
    Task<List<BillDto>> GetBillsForUnitAsync(int unitId, string? statusFilter);
    Task<BillDetailDto?> GetBillDetailAsync(int unitId, int billId);
}
