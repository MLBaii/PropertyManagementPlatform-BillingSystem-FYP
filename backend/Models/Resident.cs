namespace PropertyBill.Api.Models;

public class Resident
{
    public int ResidentId { get; set; }
    public int UnitId { get; set; }
    public int AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string NotificationPreferences { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Not part of the Chapter 4 ERD — added to support the UC-101 login flow's
    // 403 (disabled account) case. See docs/PROGRESS.md.
    public bool IsActive { get; set; } = true;

    public Unit Unit { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public ICollection<PaymentProof> PaymentProofs { get; set; } = new List<PaymentProof>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<NotificationToken> NotificationTokens { get; set; } = new List<NotificationToken>();
}
