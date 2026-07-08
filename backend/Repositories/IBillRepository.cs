using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IBillRepository
{
    Task<List<Bill>> GetByUnitIdAsync(int unitId);
    Task<Bill?> GetByIdForUnitAsync(int billId, int unitId);
}
