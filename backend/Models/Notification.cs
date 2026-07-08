namespace PropertyBill.Api.Models;

public class Notification
{
    public int NotificationId { get; set; }
    public int ResidentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resident Resident { get; set; } = null!;
}
