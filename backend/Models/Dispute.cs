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

    public Bill Bill { get; set; } = null!;
    public Resident Resident { get; set; } = null!;
}
