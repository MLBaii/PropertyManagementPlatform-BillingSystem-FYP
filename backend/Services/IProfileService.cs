using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IProfileService
{
    Task<ProfileResult> GetProfileAsync(int residentId);
    Task<ProfileResult> UpdateProfileAsync(int residentId, UpdateProfileRequest request);
    Task<ChangePasswordResult> ChangePasswordAsync(int residentId, ChangePasswordRequest request);
    Task<ProfileResult> UpdateNotificationPreferencesAsync(int residentId, NotificationPreferencesDto preferences);
}
