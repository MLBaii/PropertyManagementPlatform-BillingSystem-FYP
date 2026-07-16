namespace PropertyBill.Api.Dtos;

public class ReceiptDto
{
    public int PaymentId { get; set; }

    // Synthetic — Payment has no reference-number column of its own in the ERD, so this is
    // derived as "RCPT-{PaymentId:D6}" rather than a stored value. See ReceiptService.
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string BillReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
}
