namespace PropertyBill.Api.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }

    // Nullable: a payment isn't required to be backed by an uploaded proof (e.g. admin-recorded
    // bank transfers), and keeping this optional avoids Payment and PaymentProof forming a
    // hard mutual dependency in the model.
    public int? ProofId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";

    public Bill Bill { get; set; } = null!;
    public PaymentProof? PaymentProof { get; set; }
}
