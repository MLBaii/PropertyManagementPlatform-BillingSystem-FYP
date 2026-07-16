using System.Text.Json;

namespace PropertyBill.Api.Dtos;

public class NotificationPreferencesDto
{
    public bool PushEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool BillDueReminders { get; set; } = true;

    // Shared by ProfileService (reading a resident's own preferences) and
    // NotificationSendingService (deciding whether to send) so both agree on the same
    // malformed/empty-JSON fallback.
    public static NotificationPreferencesDto Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new NotificationPreferencesDto();
        }

        try
        {
            return JsonSerializer.Deserialize<NotificationPreferencesDto>(json) ?? new NotificationPreferencesDto();
        }
        catch (JsonException)
        {
            return new NotificationPreferencesDto();
        }
    }
}
