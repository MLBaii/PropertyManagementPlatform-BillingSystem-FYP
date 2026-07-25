using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class CreateAdditionalChargeRequest
{
    [Required, RegularExpression(@"^\d{4}-\d{2}$")]
    public string BillingPeriod { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int UnitId { get; set; }
    [Required, StringLength(80)] public string Category { get; set; } = string.Empty;
    [StringLength(250)] public string? Description { get; set; }
    [Range(typeof(decimal), "0.01", "999999.99")] public decimal Amount { get; set; }
}

public class AdminAdditionalChargeDto
{
    public int AdditionalChargeId { get; set; }
    public int UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
