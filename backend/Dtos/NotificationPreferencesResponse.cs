namespace PropertyBill.Api.Dtos;

public class NotificationPreferencesResponse
{
    public NotificationPreferencesDto NotificationPreferences { get; set; } = new();
    public string Message { get; set; } = "Notification preferences updated.";
}
