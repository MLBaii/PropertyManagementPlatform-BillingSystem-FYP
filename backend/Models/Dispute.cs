namespace PropertyBill.Api.Models;

public class Dispute
{
    public int DisputeId { get; set; }
    public int BillId { get; set; }
    public int ResidentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    // Not in the Chapter 4 ERD — added to support UC-110 (view dispute history), whose own
    // mockup caption (Figure 4.13) says "the screen shows the dispute status and admin
    // response," but the ERD's Dispute table has nowhere to store that response. Flagged
    // here for disclosure in Chapter 5/7, same as Resident.IsActive.
    public string? AdminResponse { get; set; }

    public Bill Bill { get; set; } = null!;
    public Resident Resident { get; set; } = null!;
}
