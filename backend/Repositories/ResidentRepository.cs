using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class ResidentRepository : IResidentRepository
{
    private readonly AppDbContext _context;

    public ResidentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Resident?> GetByEmailAsync(string email)
    {
        return _context.Residents.FirstOrDefaultAsync(r => r.Email == email);
    }

    public Task<Resident?> GetByIdAsync(int residentId)
    {
        return _context.Residents
            .Include(r => r.Unit)
            .ThenInclude(u => u.Property)
            .Include(r => r.Account)
            .FirstOrDefaultAsync(r => r.ResidentId == residentId);
    }

    public Task<bool> ExistsWithEmailAsync(string email, int excludingResidentId)
    {
        return _context.Residents.AnyAsync(r => r.Email == email && r.ResidentId != excludingResidentId);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
