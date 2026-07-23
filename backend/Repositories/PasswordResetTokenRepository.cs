using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        return _context.PasswordResetTokens
            .Include(t => t.Resident)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public Task<PasswordResetToken?> GetMostRecentForResidentAsync(int residentId)
    {
        return _context.PasswordResetTokens
            .Where(t => t.ResidentId == residentId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task InvalidateOtherTokensForResidentAsync(int residentId, int exceptTokenId)
    {
        await _context.PasswordResetTokens
            .Where(t => t.ResidentId == residentId && t.PasswordResetTokenId != exceptTokenId && t.UsedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.UsedAt, DateTime.UtcNow));
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
