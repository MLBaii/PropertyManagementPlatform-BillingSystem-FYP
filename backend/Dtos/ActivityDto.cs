namespace PropertyBill.Api.Dtos;

public class ActivityDto
{
    // "BillIssued" | "PaymentConfirmed".
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
}
