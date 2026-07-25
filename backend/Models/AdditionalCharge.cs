namespace PropertyBill.Api.Models;

// Temporary boundary for charges supplied by the Facility system later.
public class AdditionalCharge
{
    public int AdditionalChargeId { get; set; }
    public int UnitId { get; set; }
    public int? BillId { get; set; }
    public string BillingPeriod { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Source { get; set; } = "Admin placeholder";
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;
    public Bill? Bill { get; set; }
}
