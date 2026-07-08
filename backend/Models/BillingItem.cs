namespace PropertyBill.Api.Models;

// Admin-owned: per-property catalogue of chargeable items (e.g. maintenance fee, sinking fund),
// including the billing cadence and late-penalty rules used to generate Bills/BillLineItems.
public class BillingItem
{
    public int BillingItemId { get; set; }
    public int PropertyId { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public decimal DefaultRate { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public int BillingDay { get; set; }
    public int DueDay { get; set; }
    public decimal PenaltyRate { get; set; }
    public int GracePeriodDays { get; set; }
    public bool IsActive { get; set; } = true;

    public Property Property { get; set; } = null!;
    public ICollection<BillLineItem> BillLineItems { get; set; } = new List<BillLineItem>();
    public ICollection<UnitBillingRate> UnitBillingRates { get; set; } = new List<UnitBillingRate>();
}
