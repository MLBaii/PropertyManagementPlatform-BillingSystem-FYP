namespace PropertyBill.Api.Dtos;

public class BillDetailDto
{
    public int BillId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysUntilDue { get; set; }
    public List<BillLineItemDto> LineItems { get; set; } = new();
}
