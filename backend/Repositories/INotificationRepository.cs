using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);

    Task<List<Notification>> GetByResidentIdAsync(int residentId);

    // Scoped by residentId, same pattern as BillRepository.GetByIdForUnitAsync — a
    // notification belonging to someone else simply doesn't match, so "doesn't exist" and
    // "isn't yours" are indistinguishable (uniform 404) rather than leaking a 403.
    Task<Notification?> GetByIdForResidentAsync(int notificationId, int residentId);

    Task SaveChangesAsync();

    Task MarkAllAsReadAsync(int residentId);
}
