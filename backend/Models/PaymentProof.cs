namespace PropertyBill.Api.Models;

public class PaymentProof
{
    public int PaymentProofId { get; set; }
    public int BillId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Bill Bill { get; set; } = null!;
}
