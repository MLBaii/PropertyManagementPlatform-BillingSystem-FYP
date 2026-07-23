using System.ComponentModel.DataAnnotations;
namespace PropertyBill.Api.Dtos;
public class CreateAdminUnitRequest
{
    [Required, StringLength(30)] public string UnitNumber { get; set; } = string.Empty;
    [Range(0, 200)] public int Floor { get; set; }
    [Required, StringLength(80)] public string Type { get; set; } = string.Empty;
}
