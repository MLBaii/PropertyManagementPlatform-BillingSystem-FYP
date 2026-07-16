namespace PropertyBill.Api.Services;

public record ExpoPushResult(string ExpoPushToken, bool Success, string? ErrorCode);

public interface IExpoPushService
{
    Task<List<ExpoPushResult>> SendAsync(List<string> expoPushTokens, string title, string body, string? deepLink);
}
