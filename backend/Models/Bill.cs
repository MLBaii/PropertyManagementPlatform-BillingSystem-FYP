namespace PropertyBill.Api.Models;

public class Bill
{
    public int BillId { get; set; }
    public int UnitId { get; set; }
    public string BillingPeriod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime DueDate { get; set; }

    public Unit Unit { get; set; } = null!;
    public ICollection<BillLineItem> BillLineItems { get; set; } = new List<BillLineItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
}
