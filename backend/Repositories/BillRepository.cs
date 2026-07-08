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
}
