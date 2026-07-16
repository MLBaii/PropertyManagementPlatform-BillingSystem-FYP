using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class SubmitDisputeRequest
{
    [Required(ErrorMessage = "A bill is required.")]
    public int BillId { get; set; }

    [Required(ErrorMessage = "Please provide a dispute reason of at least 20 characters.")]
    [MinLength(20, ErrorMessage = "Please provide a dispute reason of at least 20 characters.")]
    public string Reason { get; set; } = string.Empty;
}
