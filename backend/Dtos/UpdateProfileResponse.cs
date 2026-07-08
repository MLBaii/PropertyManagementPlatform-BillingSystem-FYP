namespace PropertyBill.Api.Dtos;

public class UpdateProfileResponse
{
    public ProfileDto Profile { get; set; } = new();
    public string Message { get; set; } = "Profile updated successfully!";
}
