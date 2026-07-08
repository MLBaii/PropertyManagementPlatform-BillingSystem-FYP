namespace PropertyBill.Api.Models;

public class Dispute
{
    public int DisputeId { get; set; }
    public int BillId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public Bill Bill { get; set; } = null!;
}
