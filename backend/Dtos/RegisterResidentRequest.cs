using System.ComponentModel.DataAnnotations;

namespace PropertyBill.Api.Dtos;

public class RegisterResidentRequest
{
    [Range(1, int.MaxValue)] public int UnitId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
    [StringLength(30)] public string? PhoneNumber { get; set; }
}
