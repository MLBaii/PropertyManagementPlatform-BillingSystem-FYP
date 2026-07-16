using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IPaymentRepository
{
    Task<List<Payment>> GetConfirmedByUnitIdAsync(int unitId);

    // Scoped by unitId, same pattern as BillRepository.GetByIdForUnitAsync, plus a Status
    // check — receipts only exist for Confirmed payments, so a Pending/unconfirmed payment
    // 404s exactly like one belonging to another unit does.
    Task<Payment?> GetConfirmedByIdForUnitAsync(int paymentId, int unitId);
}
