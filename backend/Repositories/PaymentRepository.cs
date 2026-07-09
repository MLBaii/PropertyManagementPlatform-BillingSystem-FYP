using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Payment>> GetConfirmedByUnitIdAsync(int unitId)
    {
        return _context.Payments
            .Include(p => p.Bill)
            .Where(p => p.Bill.UnitId == unitId && p.Status == "Confirmed")
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}
