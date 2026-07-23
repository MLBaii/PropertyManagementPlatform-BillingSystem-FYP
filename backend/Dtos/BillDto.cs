namespace PropertyBill.Api.Dtos;

public class BillDto
{
    public int BillId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal OutstandingBalance { get; set; }

    // Computed, not the raw stored column — see BillService.ComputeEffectiveStatus. Purely
    // payment-side: one of "Unpaid" | "Overdue" | "Paid" | "ProofSubmitted". Never folds in
    // dispute state — see ActiveDisputeStatus for that.
    public string Status { get; set; } = string.Empty;

    // Null unless the bill has a dispute that's still Open ("Disputed") or UnderReview
    // ("PendingDispute") — see BillService.ComputeActiveDisputeStatus. A Resolved/Rejected
    // dispute leaves this null, since it's no longer "active". Rendered as a second badge
    // alongside Status, not folded into it, so the resident always sees the real payment
    // state even while a dispute is in progress.
    public string? ActiveDisputeStatus { get; set; }

    // Positive = days left until due; negative = days overdue. Not meaningful once Paid.
    public int DaysUntilDue { get; set; }
}
