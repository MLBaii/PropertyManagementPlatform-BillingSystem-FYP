using System.ComponentModel.DataAnnotations;
namespace PropertyBill.Api.Dtos;
public class GenerateBillsRequest { [Required, RegularExpression(@"^\d{4}-\d{2}$")] public string BillingPeriod { get; set; } = string.Empty; public DateTime DueDate { get; set; } }
