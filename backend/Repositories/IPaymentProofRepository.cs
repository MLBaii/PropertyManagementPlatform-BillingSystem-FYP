using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IPaymentProofRepository
{
    Task<List<Bill>> GetBillsByIdsForUnitAsync(List<int> billIds, int unitId);

    // Persists one PaymentProof row per uploaded file plus one Pending Payment per tagged bill
    // (the tag-link, since the ERD has no direct PaymentProof<->Bill relation) — all in one
    // SaveChanges call. The Payment rows are linked to proofs[0] (the "primary" file) only, so
    // a 3-file submission doesn't triple the pending amount owed on each tagged bill.
    Task<List<PaymentProof>> CreateAsync(List<PaymentProof> proofs, List<Bill> taggedBills);

    Task<List<PaymentProof>> GetByResidentIdAsync(int residentId);
}
