using System.ComponentModel.DataAnnotations;
namespace PropertyBill.Api.Dtos;
public class AdminLoginRequest { [Required] public string Username { get; set; } = string.Empty; [Required] public string Password { get; set; } = string.Empty; }
