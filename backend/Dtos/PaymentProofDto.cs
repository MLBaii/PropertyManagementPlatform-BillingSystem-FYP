namespace PropertyBill.Api.Dtos;

public class PaymentProofDto
{
    public int ProofId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    // "Pending" | "Approved" | "Rejected" — raw stored value; admin review flow (out of
    // scope here) is what would ever move it off "Pending".
    public string Status { get; set; } = string.Empty;
    public string? AdminRemarks { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public List<TaggedBillDto> TaggedBills { get; set; } = new();
}
