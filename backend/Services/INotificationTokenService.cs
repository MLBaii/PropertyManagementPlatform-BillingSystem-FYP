using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface INotificationTokenService
{
    Task<NotificationTokenDto> RegisterAsync(int residentId, RegisterNotificationTokenRequest request);
}
