namespace PropertyBill.Api.Dtos;

public class TaggedBillDto
{
    public int BillId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
