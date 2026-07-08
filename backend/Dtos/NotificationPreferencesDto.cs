namespace PropertyBill.Api.Dtos;

public class NotificationPreferencesDto
{
    public bool PushEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool BillDueReminders { get; set; } = true;
}
