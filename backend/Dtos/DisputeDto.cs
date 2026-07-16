namespace PropertyBill.Api.Dtos;

public class DisputeDto
{
    public int DisputeId { get; set; }
    public int BillId { get; set; }
    public string BillReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    // "Open" | "UnderReview" | "Resolved" | "Rejected"
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Not in the Chapter 4 ERD — see Dispute.AdminResponse. Null until an admin portal
    // (out of scope here) ever writes one.
    public string? AdminResponse { get; set; }
}
