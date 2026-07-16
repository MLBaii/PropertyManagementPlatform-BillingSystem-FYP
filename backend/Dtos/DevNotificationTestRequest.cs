namespace PropertyBill.Api.Dtos;

public class DevNotificationTestRequest
{
    public int ResidentId { get; set; }

    // "BillIssued" | "BillOverdue" | "PaymentConfirmed" | "DueReminder" — see
    // NotificationSendingService.SampleContent for the canned title/body per type.
    public string Type { get; set; } = "BillIssued";
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DeepLink { get; set; }
}
