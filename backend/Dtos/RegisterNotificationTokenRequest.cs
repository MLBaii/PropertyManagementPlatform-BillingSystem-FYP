namespace PropertyBill.Api.Dtos;

public class RegisterNotificationTokenRequest
{
    public string ExpoPushToken { get; set; } = string.Empty;
    public string DeviceInfo { get; set; } = string.Empty;
}
