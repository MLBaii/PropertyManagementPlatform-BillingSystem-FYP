namespace PropertyBill.Api.Models;

// Per the ERD, a proof is submitted by a Resident directly (not scoped to one Bill) —
// admins tag it to the relevant Payment(s) during review.
public class PaymentProof
{
    public int ProofId { get; set; }
    public int ResidentId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Status { get; set; } = "Pending";
    public string? AdminRemarks { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Resident Resident { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
