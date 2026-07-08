namespace PropertyBill.Api.Models;

public class NotificationToken
{
    public int TokenId { get; set; }
    public int ResidentId { get; set; }
    public string ExpoPushToken { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public Resident Resident { get; set; } = null!;
}
