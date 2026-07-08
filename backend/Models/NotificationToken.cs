namespace PropertyBill.Api.Models;

public class NotificationToken
{
    public int NotificationTokenId { get; set; }
    public int ResidentId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resident Resident { get; set; } = null!;
}
