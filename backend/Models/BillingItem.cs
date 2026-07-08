namespace PropertyBill.Api.Models;

// Admin-owned: catalogue of billable items (e.g. maintenance fee, sinking fund).
public class BillingItem
{
    public int BillingItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultAmount { get; set; }

    public ICollection<BillLineItem> BillLineItems { get; set; } = new List<BillLineItem>();
    public ICollection<UnitBillingRate> UnitBillingRates { get; set; } = new List<UnitBillingRate>();
}
