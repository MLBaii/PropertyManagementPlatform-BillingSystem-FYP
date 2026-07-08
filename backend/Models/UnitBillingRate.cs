namespace PropertyBill.Api.Models;

// Admin-owned: per-unit override of a BillingItem's rate (e.g. by unit size/type).
public class UnitBillingRate
{
    public int UnitBillingRateId { get; set; }
    public int UnitId { get; set; }
    public int BillingItemId { get; set; }
    public decimal Rate { get; set; }

    public Unit Unit { get; set; } = null!;
    public BillingItem BillingItem { get; set; } = null!;
}
