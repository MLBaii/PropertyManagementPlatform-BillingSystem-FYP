using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class PaymentProofRepository : IPaymentProofRepository
{
    private readonly AppDbContext _context;

    public PaymentProofRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Bill>> GetBillsByIdsForUnitAsync(List<int> billIds, int unitId)
    {
        // Scoped by unitId, same as GetByIdForUnitAsync — a billId belonging to another
        // resident's unit simply won't come back, so it can't be tagged onto someone else's proof.
        return _context.Bills
            .Where(b => billIds.Contains(b.BillId) && b.UnitId == unitId)
            .ToListAsync();
    }

    public async Task<List<PaymentProof>> CreateAsync(List<PaymentProof> proofs, List<Bill> taggedBills)
    {
        _context.PaymentProofs.AddRange(proofs);

        var primaryProof = proofs[0];
        foreach (var bill in taggedBills)
        {
            bill.Status = "ProofSubmitted";
            _context.Payments.Add(new Payment
            {
                Bill = bill,
                PaymentProof = primaryProof,
                Amount = bill.OutstandingBalance,
                PaymentDate = DateTime.UtcNow,
                Channel = "Proof Upload",
                Status = "Pending",
            });
        }

        await _context.SaveChangesAsync();
        return proofs;
    }

    public Task<List<PaymentProof>> GetByResidentIdAsync(int residentId)
    {
        return _context.PaymentProofs
            .Include(pp => pp.Payments)
            .ThenInclude(p => p.Bill)
            .Where(pp => pp.ResidentId == residentId)
            .OrderByDescending(pp => pp.SubmittedAt)
            .ToListAsync();
    }
}
