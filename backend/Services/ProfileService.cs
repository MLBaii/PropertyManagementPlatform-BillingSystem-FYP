using System.Text.Json;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class ProfileService : IProfileService
{
    private readonly IResidentRepository _residentRepository;

    public ProfileService(IResidentRepository residentRepository)
    {
        _residentRepository = residentRepository;
    }

    public async Task<ProfileResult> GetProfileAsync(int residentId)
    {
        var resident = await _residentRepository.GetByIdAsync(residentId);
        return resident is null ? ProfileResult.NotFound() : ProfileResult.Success(ToDto(resident));
    }

    public async Task<ProfileResult> UpdateProfileAsync(int residentId, UpdateProfileRequest request)
    {
        var resident = await _residentRepository.GetByIdAsync(residentId);
        if (resident is null)
        {
            return ProfileResult.NotFound();
        }

        if (!string.Equals(resident.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && await _residentRepository.ExistsWithEmailAsync(request.Email, residentId))
        {
            return ProfileResult.EmailTaken();
        }

        resident.Name = request.Name;
        resident.PhoneNumber = request.PhoneNumber;
        resident.Email = request.Email;

        await _residentRepository.SaveChangesAsync();

        return ProfileResult.Success(ToDto(resident));
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(int residentId, ChangePasswordRequest request)
    {
        var resident = await _residentRepository.GetByIdAsync(residentId);
        if (resident is null)
        {
            return ChangePasswordResult.NotFound();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, resident.PasswordHash))
        {
            return ChangePasswordResult.InvalidCurrentPassword();
        }

        resident.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _residentRepository.SaveChangesAsync();

        return ChangePasswordResult.Success();
    }

    public async Task<ProfileResult> UpdateNotificationPreferencesAsync(int residentId, NotificationPreferencesDto preferences)
    {
        var resident = await _residentRepository.GetByIdAsync(residentId);
        if (resident is null)
        {
            return ProfileResult.NotFound();
        }

        resident.NotificationPreferences = JsonSerializer.Serialize(preferences);
        await _residentRepository.SaveChangesAsync();

        return ProfileResult.Success(ToDto(resident));
    }

    private static ProfileDto ToDto(Resident resident)
    {
        return new ProfileDto
        {
            Name = resident.Name,
            Email = resident.Email,
            PhoneNumber = resident.PhoneNumber,
            NotificationPreferences = ParsePreferences(resident.NotificationPreferences),
            UnitNumber = resident.Unit.UnitNumber,
            Floor = resident.Unit.Floor,
            PropertyName = resident.Unit.Property.Name,
        };
    }

    private static NotificationPreferencesDto ParsePreferences(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new NotificationPreferencesDto();
        }

        try
        {
            return JsonSerializer.Deserialize<NotificationPreferencesDto>(json) ?? new NotificationPreferencesDto();
        }
        catch (JsonException)
        {
            return new NotificationPreferencesDto();
        }
    }
}
