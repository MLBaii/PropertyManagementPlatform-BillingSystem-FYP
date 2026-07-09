using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class BillRepository : IBillRepository
{
    private readonly AppDbContext _context;

    public BillRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Bill>> GetByUnitIdAsync(int unitId)
    {
        return _context.Bills
            .Where(b => b.UnitId == unitId)
            .OrderByDescending(b => b.IssueDate)
            .ToListAsync();
    }

    public Task<Bill?> GetByIdForUnitAsync(int billId, int unitId)
    {
        // Filtering by unitId here (not just billId) means a bill belonging to
        // another resident's unit simply doesn't match — no separate ownership
        // check needed, and the caller can't distinguish "doesn't exist" from
        // "isn't yours" by response shape.
        return _context.Bills
            .Include(b => b.BillLineItems)
            .Include(b => b.Unit)
            .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(b => b.BillId == billId && b.UnitId == unitId);
    }
}
