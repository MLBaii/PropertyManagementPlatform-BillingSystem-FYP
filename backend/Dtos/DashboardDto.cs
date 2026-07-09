namespace PropertyBill.Api.Dtos;

public class DashboardDto
{
    public string UnitNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;

    // Sum of Bill.OutstandingBalance across the unit's bills — computed live rather than
    // trusting Account.CumulativeArrears, which nothing in this project keeps in sync as
    // bills are added (same reasoning as BillService.ComputeEffectiveStatus for "Overdue").
    public decimal TotalOutstanding { get; set; }

    // Sum of confirmed Payment.Amount for the unit's bills.
    public decimal TotalPaid { get; set; }

    // Account.CreditBalance, trusted as stored — nothing else in this schema computes it.
    public decimal CreditBalance { get; set; }

    public List<ActivityDto> RecentActivity { get; set; } = new();
}
