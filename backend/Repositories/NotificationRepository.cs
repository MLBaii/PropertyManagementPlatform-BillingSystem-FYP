using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public Task<List<Notification>> GetByResidentIdAsync(int residentId)
    {
        return _context.Notifications
            .Where(n => n.ResidentId == residentId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public Task<Notification?> GetByIdForResidentAsync(int notificationId, int residentId)
    {
        return _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.ResidentId == residentId);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task MarkAllAsReadAsync(int residentId)
    {
        return _context.Notifications
            .Where(n => n.ResidentId == residentId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
