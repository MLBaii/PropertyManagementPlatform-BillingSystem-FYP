namespace PropertyBill.Api.Models;

public class Bill
{
    public int BillId { get; set; }
    public int AccountId { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Account Account { get; set; } = null!;
    public ICollection<BillLineItem> BillLineItems { get; set; } = new List<BillLineItem>();
    public ICollection<PaymentProof> PaymentProofs { get; set; } = new List<PaymentProof>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
}
