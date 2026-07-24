using System.ComponentModel.DataAnnotations;
namespace PropertyBill.Api.Dtos;
public class CreateBillingItemRequest { [Required, StringLength(100)] public string ChargeType { get; set; } = string.Empty; [Range(typeof(decimal), "0", "999999")] public decimal DefaultRate { get; set; } [Required] public string Frequency { get; set; } = "Monthly"; [Range(1,31)] public int BillingDay { get; set; } [Range(1,31)] public int DueDay { get; set; } [Range(typeof(decimal), "0", "100")] public decimal PenaltyRate { get; set; } [Range(0,365)] public int GracePeriodDays { get; set; } }
