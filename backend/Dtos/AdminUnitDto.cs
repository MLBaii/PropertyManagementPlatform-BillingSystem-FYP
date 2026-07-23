namespace PropertyBill.Api.Dtos;
public class AdminUnitDto { public int UnitId { get; set; } public string UnitNumber { get; set; } = string.Empty; public int Floor { get; set; } public string Type { get; set; } = string.Empty; public bool IsActive { get; set; } public int ResidentCount { get; set; } }
