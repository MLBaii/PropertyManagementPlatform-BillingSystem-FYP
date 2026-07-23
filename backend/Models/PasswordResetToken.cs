namespace PropertyBill.Api.Models;

// Not in the Chapter 4 ERD — added to support UC-101 A1 (forgot password). Same disclosure
// category as Resident.IsActive and Dispute.AdminResponse; see docs/SCHEMA.md.
public class PasswordResetToken
{
    public int PasswordResetTokenId { get; set; }
    public int ResidentId { get; set; }

    // SHA-256 hex digest of the raw token emailed to the resident — the raw value itself is
    // never persisted, only ever held in memory long enough to email and hash it.
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Resident Resident { get; set; } = null!;
}
