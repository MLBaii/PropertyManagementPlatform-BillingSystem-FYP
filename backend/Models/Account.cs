namespace PropertyBill.Api.Models;

// Corrected per docs/SCHEMA.md: Account links to Unit, not Resident.
public class Account
{
    public int AccountId { get; set; }
    public int UnitId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
