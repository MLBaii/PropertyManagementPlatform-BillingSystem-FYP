using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class ResetResidentPasswordRequest
{
    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}
