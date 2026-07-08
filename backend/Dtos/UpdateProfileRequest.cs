using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
