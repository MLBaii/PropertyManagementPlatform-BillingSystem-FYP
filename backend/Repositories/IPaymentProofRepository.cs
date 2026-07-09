using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IPaymentProofRepository
{
    Task<List<Bill>> GetBillsByIdsForUnitAsync(List<int> billIds, int unitId);

    // Persists the proof plus one Pending Payment per tagged bill (the tag-link, since the
    // ERD has no direct PaymentProof<->Bill relation) and flips each tagged bill's Status to
    // "ProofSubmitted" — all in one SaveChanges call.
    Task<PaymentProof> CreateAsync(PaymentProof proof, List<Bill> taggedBills);

    Task<List<PaymentProof>> GetByResidentIdAsync(int residentId);
}
