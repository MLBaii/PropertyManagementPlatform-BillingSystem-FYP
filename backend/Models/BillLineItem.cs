namespace PropertyBill.Api.Models;

public class BillLineItem
{
    public int BillLineItemId { get; set; }
    public int BillId { get; set; }
    public int? BillingItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public Bill Bill { get; set; } = null!;
    public BillingItem? BillingItem { get; set; }
}
