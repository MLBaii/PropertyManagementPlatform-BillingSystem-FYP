namespace PropertyBill.Api.Dtos;

public class ReceiptDetailDto
{
    public int PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string BillReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;

    // Same reasoning as BillDetailDto — the PDF masthead needs unit/property info the list
    // response doesn't. Resident name deliberately isn't here: the frontend already has it
    // from AuthContext (same pattern generateAndShareBillPdf already uses for the Bill PDF).
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
}
