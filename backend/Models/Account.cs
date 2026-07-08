namespace PropertyBill.Api.Models;

// Per the Chapter 4 ERD, Account is keyed off Resident (Resident.AccountId), not Unit —
// it tracks one resident's running arrears/credit balance rather than a per-unit ledger.
public class Account
{
    public int AccountId { get; set; }
    public decimal CumulativeArrears { get; set; }
    public decimal CreditBalance { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public Resident? Resident { get; set; }
}
