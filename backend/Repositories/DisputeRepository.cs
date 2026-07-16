using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class DisputeRepository : IDisputeRepository
{
    private static readonly string[] ActiveStatuses = { "Open", "UnderReview" };

    private readonly AppDbContext _context;

    public DisputeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Dispute?> GetActiveForBillAsync(int billId)
    {
        return _context.Disputes
            .Include(d => d.Bill)
            .FirstOrDefaultAsync(d => d.BillId == billId && ActiveStatuses.Contains(d.Status));
    }

    public async Task<Dispute> CreateAsync(Dispute dispute)
    {
        _context.Disputes.Add(dispute);
        await _context.SaveChangesAsync();
        return dispute;
    }

    public Task<List<Dispute>> GetByResidentIdAsync(int residentId, string? statusFilter)
    {
        var query = _context.Disputes
            .Include(d => d.Bill)
            .Where(d => d.ResidentId == residentId);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            // .ToLower() on both sides translates to SQL LOWER() — case-insensitive without
            // depending on a provider-specific function, same case-insensitivity the Bills
            // endpoint's ?status= filter already gives residents.
            var normalizedFilter = statusFilter.ToLower();
            query = query.Where(d => d.Status.ToLower() == normalizedFilter);
        }

        return query.OrderByDescending(d => d.SubmittedAt).ToListAsync();
    }

    public Task<Dispute?> GetByIdForResidentAsync(int disputeId, int residentId)
    {
        return _context.Disputes
            .Include(d => d.Bill)
            .FirstOrDefaultAsync(d => d.DisputeId == disputeId && d.ResidentId == residentId);
    }
}
