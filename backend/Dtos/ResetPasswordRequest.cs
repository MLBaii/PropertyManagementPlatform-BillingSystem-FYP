using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Reset code is required.")]
    public string Token { get; set; } = string.Empty;

    // Exact wording per UC-102 C3 / the documented reset-screen validation message.
    [Required(ErrorMessage = "Password must be at least 8 characters long.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
}
