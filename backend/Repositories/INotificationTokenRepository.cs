using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface INotificationTokenRepository
{
    // Upserts by the token string itself, not by (ResidentId, DeviceInfo) — if the same
    // device's token was previously registered under a different resident (e.g. a shared
    // device, prior resident logged out), the token is reassigned to whoever just registered
    // it, since only the current session on that device should receive its pushes.
    Task<NotificationToken> UpsertAsync(int residentId, string expoPushToken, string deviceInfo);

    Task<List<NotificationToken>> GetActiveTokensForResidentAsync(int residentId);

    Task DeactivateTokensAsync(List<string> expoPushTokens);
}
