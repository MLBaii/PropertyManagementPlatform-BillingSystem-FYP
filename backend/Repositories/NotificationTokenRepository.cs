using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class NotificationTokenRepository : INotificationTokenRepository
{
    private readonly AppDbContext _context;

    public NotificationTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationToken> UpsertAsync(int residentId, string expoPushToken, string deviceInfo)
    {
        var existing = await _context.NotificationTokens
            .FirstOrDefaultAsync(t => t.ExpoPushToken == expoPushToken);

        if (existing is not null)
        {
            existing.ResidentId = residentId;
            existing.DeviceInfo = deviceInfo;
            existing.IsActive = true;
            existing.RegisteredAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var token = new NotificationToken
        {
            ResidentId = residentId,
            ExpoPushToken = expoPushToken,
            DeviceInfo = deviceInfo,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow,
        };
        _context.NotificationTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public Task<List<NotificationToken>> GetActiveTokensForResidentAsync(int residentId)
    {
        return _context.NotificationTokens
            .Where(t => t.ResidentId == residentId && t.IsActive)
            .ToListAsync();
    }

    public Task DeactivateTokensAsync(List<string> expoPushTokens)
    {
        return _context.NotificationTokens
            .Where(t => expoPushTokens.Contains(t.ExpoPushToken))
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));
    }
}
