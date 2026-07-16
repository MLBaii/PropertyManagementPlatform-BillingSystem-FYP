using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetHistoryAsync(int residentId);

    // Returns false if the notification doesn't exist or doesn't belong to this resident.
    Task<bool> MarkAsReadAsync(int notificationId, int residentId);

    Task MarkAllAsReadAsync(int residentId);
}
