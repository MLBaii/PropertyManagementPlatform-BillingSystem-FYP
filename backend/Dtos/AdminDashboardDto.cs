namespace PropertyBill.Api.Dtos;

public class AdminDashboardDto
{
    public string PropertyName { get; set; } = string.Empty;
    public int ActiveUnits { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal ConfirmedPayments { get; set; }
    public int PendingPaymentProofs { get; set; }
    public int OpenDisputes { get; set; }
}
