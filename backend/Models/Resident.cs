namespace PropertyBill.Api.Models;

public class Resident
{
    public int ResidentId { get; set; }
    public int UnitId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<NotificationToken> NotificationTokens { get; set; } = new List<NotificationToken>();
}
