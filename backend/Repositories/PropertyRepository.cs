using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly AppDbContext _context;

    public PropertyRepository(AppDbContext context)
    {
        _context = context;
    }

    // Single-property system (see DbSeeder) — no filtering criteria needed yet.
    public Task<Property?> GetFirstAsync()
    {
        return _context.Properties.FirstOrDefaultAsync();
    }
}
