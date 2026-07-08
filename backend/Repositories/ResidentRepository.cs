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
}
