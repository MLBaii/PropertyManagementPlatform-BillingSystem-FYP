using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class NotificationSendingService : INotificationSendingService
{
    private readonly IResidentRepository _residentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationTokenRepository _tokenRepository;
    private readonly IExpoPushService _expoPushService;
    private readonly ILogger<NotificationSendingService> _logger;

    public NotificationSendingService(
        IResidentRepository residentRepository,
        INotificationRepository notificationRepository,
        INotificationTokenRepository tokenRepository,
        IExpoPushService expoPushService,
        ILogger<NotificationSendingService> logger)
    {
        _residentRepository = residentRepository;
        _notificationRepository = notificationRepository;
        _tokenRepository = tokenRepository;
        _expoPushService = expoPushService;
        _logger = logger;
    }

    public async Task<bool> SendAsync(int residentId, string type, string title, string body, string? deepLink)
    {
        var resident = await _residentRepository.GetByIdAsync(residentId);
        if (resident is null)
        {
            return false;
        }

        var preferences = NotificationPreferencesDto.Parse(resident.NotificationPreferences);

        // "DueReminder" is the one category with its own dedicated toggle — respecting it
        // means not sending at all (no in-app record either), since the resident explicitly
        // asked not to be reminded about this. Other types (bill issued, overdue, payment
        // confirmed) aren't gated by a specific flag, so they're always recorded in-app;
        // PushEnabled below only gates whether a *device push* goes out for them.
        if (string.Equals(type, "DueReminder", StringComparison.OrdinalIgnoreCase) && !preferences.BillDueReminders)
        {
            return true;
        }

        var notification = new Notification
        {
            ResidentId = residentId,
            Type = type,
            Title = title,
            Body = body,
            DeepLink = deepLink,
            IsRead = false,
            SentAt = DateTime.UtcNow,
        };
        await _notificationRepository.CreateAsync(notification);

        if (!preferences.PushEnabled)
        {
            return true;
        }

        var tokens = await _tokenRepository.GetActiveTokensForResidentAsync(residentId);
        if (tokens.Count == 0)
        {
            return true;
        }

        var results = await _expoPushService.SendAsync(
            tokens.Select(t => t.ExpoPushToken).ToList(), title, body, deepLink);

        var invalidTokens = results
            .Where(r => !r.Success && string.Equals(r.ErrorCode, "DeviceNotRegistered", StringComparison.OrdinalIgnoreCase))
            .Select(r => r.ExpoPushToken)
            .ToList();
        if (invalidTokens.Count > 0)
        {
            _logger.LogWarning(
                "Deactivating {Count} notification token(s) for resident {ResidentId}: no longer registered",
                invalidTokens.Count, residentId);
            await _tokenRepository.DeactivateTokensAsync(invalidTokens);
        }

        foreach (var failure in results.Where(r => !r.Success))
        {
            _logger.LogWarning(
                "Push delivery failed for resident {ResidentId}: {ErrorCode}", residentId, failure.ErrorCode);
        }

        return true;
    }
}
