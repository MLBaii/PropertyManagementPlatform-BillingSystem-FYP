namespace PropertyBill.Api.Dtos;

public class ProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public NotificationPreferencesDto NotificationPreferences { get; set; } = new();

    // Read-only unit details — not editable via PUT /api/residents/profile.
    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string PropertyName { get; set; } = string.Empty;
}
