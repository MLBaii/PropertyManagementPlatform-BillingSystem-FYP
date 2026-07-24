namespace PropertyBill.Api.Dtos;
public class AdminAuditLogDto { public int AuditLogId { get; set; } public string AdminUsername { get; set; } = string.Empty; public string ActionType { get; set; } = string.Empty; public string AffectedEntity { get; set; } = string.Empty; public int? AffectedEntityId { get; set; } public string? Description { get; set; } public DateTime Timestamp { get; set; } }
