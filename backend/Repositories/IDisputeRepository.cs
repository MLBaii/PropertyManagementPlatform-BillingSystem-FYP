using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IDisputeRepository
{
    // "Active" = Status is "Open" or "UnderReview" — the one-active-dispute-per-bill rule.
    Task<Dispute?> GetActiveForBillAsync(int billId);

    Task<Dispute> CreateAsync(Dispute dispute);

    Task<List<Dispute>> GetByResidentIdAsync(int residentId, string? statusFilter);

    // Scoped by residentId, same pattern as BillRepository.GetByIdForUnitAsync — a dispute
    // belonging to someone else simply doesn't match, so "doesn't exist" and "isn't yours"
    // are indistinguishable (uniform 404) rather than leaking a 403.
    Task<Dispute?> GetByIdForResidentAsync(int disputeId, int residentId);
}
