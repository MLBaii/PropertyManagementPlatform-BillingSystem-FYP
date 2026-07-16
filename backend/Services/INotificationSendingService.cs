namespace PropertyBill.Api.Services;

public interface INotificationSendingService
{
    // Creates the in-app Notification record and, preferences permitting, sends a push to
    // the resident's active device tokens. Returns false if the resident doesn't exist.
    Task<bool> SendAsync(int residentId, string type, string title, string body, string? deepLink);
}
