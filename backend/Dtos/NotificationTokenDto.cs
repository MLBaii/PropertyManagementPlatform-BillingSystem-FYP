namespace PropertyBill.Api.Dtos;

public class NotificationTokenDto
{
    public int TokenId { get; set; }
    public string ExpoPushToken { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}
