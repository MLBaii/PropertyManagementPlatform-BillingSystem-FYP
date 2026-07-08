namespace PropertyBill.Api.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";

    public Account Account { get; set; } = null!;
}
