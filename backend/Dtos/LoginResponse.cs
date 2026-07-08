namespace PropertyBill.Api.Dtos;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int UnitId { get; set; }
}
