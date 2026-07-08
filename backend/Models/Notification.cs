namespace PropertyBill.Api.Models;

public class Notification
{
    public int NotificationId { get; set; }
    public int ResidentId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? DeepLink { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Resident Resident { get; set; } = null!;
}
