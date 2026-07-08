namespace PropertyBill.Api.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int AdminUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string AffectedEntity { get; set; } = string.Empty;
    public int? AffectedEntityId { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AdminUser AdminUser { get; set; } = null!;
}
