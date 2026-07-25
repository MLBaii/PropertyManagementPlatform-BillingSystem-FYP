namespace PropertyBill.Api.Dtos;

public class AdminDisputeDto
{
    public int DisputeId { get; set; }
    public string ResidentName { get; set; } = string.Empty;
    public string UnitNumber { get; set; } = string.Empty;
    public string BillReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? AdminResponse { get; set; }
}
