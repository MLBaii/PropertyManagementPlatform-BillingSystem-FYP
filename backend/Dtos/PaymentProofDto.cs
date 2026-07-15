namespace PropertyBill.Api.Dtos;

public class PaymentProofDto
{
    // Id of the submission's primary (first-uploaded) file — one PaymentProof row is created
    // per file (see PaymentProofService), so this identifies the group, not a single row.
    public int ProofId { get; set; }
    public List<PaymentProofFileDto> Files { get; set; } = new();

    // "Pending" | "Approved" | "Rejected" — raw stored value; admin review flow (out of
    // scope here) is what would ever move it off "Pending".
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public List<TaggedBillDto> TaggedBills { get; set; } = new();
}
