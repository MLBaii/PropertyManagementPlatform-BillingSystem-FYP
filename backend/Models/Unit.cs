namespace PropertyBill.Api.Models;

public class Unit
{
    public int UnitId { get; set; }
    public int PropertyId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;

    public Property Property { get; set; } = null!;
    public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    public Account? Account { get; set; }
    public ICollection<UnitBillingRate> UnitBillingRates { get; set; } = new List<UnitBillingRate>();
}
