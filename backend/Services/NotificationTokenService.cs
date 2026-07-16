using PropertyBill.Api.Dtos;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class NotificationTokenService : INotificationTokenService
{
    private readonly INotificationTokenRepository _tokenRepository;

    public NotificationTokenService(INotificationTokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<NotificationTokenDto> RegisterAsync(int residentId, RegisterNotificationTokenRequest request)
    {
        var token = await _tokenRepository.UpsertAsync(residentId, request.ExpoPushToken, request.DeviceInfo);
        return new NotificationTokenDto
        {
            TokenId = token.TokenId,
            ExpoPushToken = token.ExpoPushToken,
            IsActive = token.IsActive,
            RegisteredAt = token.RegisteredAt,
        };
    }
}
