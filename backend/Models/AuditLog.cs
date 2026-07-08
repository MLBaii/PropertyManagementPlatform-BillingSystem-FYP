namespace PropertyBill.Api.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AdminUser AdminUser { get; set; } = null!;
}
