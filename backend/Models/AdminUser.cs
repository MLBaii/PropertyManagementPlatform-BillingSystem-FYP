namespace PropertyBill.Api.Models;

// Corrected per docs/SCHEMA.md: AdminUser has NO AccountId FK.
public class AdminUser
{
    public int AdminUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
