namespace PropertyBill.Api.Models;

public class Unit
{
    public int UnitId { get; set; }
    public int PropertyId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Property Property { get; set; } = null!;
    public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<UnitBillingRate> UnitBillingRates { get; set; } = new List<UnitBillingRate>();
}
