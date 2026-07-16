using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> GetHistoryAsync(int residentId)
    {
        var notifications = await _notificationRepository.GetByResidentIdAsync(residentId);
        return notifications.Select(ToDto).ToList();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int residentId)
    {
        var notification = await _notificationRepository.GetByIdForResidentAsync(notificationId, residentId);
        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;
        await _notificationRepository.SaveChangesAsync();
        return true;
    }

    public Task MarkAllAsReadAsync(int residentId)
    {
        return _notificationRepository.MarkAllAsReadAsync(residentId);
    }

    private static NotificationDto ToDto(Notification notification)
    {
        return new NotificationDto
        {
            NotificationId = notification.NotificationId,
            Type = notification.Type,
            Title = notification.Title,
            Body = notification.Body,
            DeepLink = notification.DeepLink,
            IsRead = notification.IsRead,
            SentAt = notification.SentAt,
        };
    }
}
