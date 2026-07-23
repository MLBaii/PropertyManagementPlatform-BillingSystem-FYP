using System.ComponentModel.DataAnnotations;
namespace PropertyBill.Api.Dtos;
public class ReviewPaymentProofRequest { [Required, RegularExpression("Confirmed|Rejected")] public string Decision { get; set; } = string.Empty; [StringLength(500)] public string? AdminRemarks { get; set; } }
