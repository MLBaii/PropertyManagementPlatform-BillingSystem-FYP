using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Route("api/residents")]
[Authorize]
public class ResidentsController : ControllerBase
{
    private readonly IBillService _billService;
    private readonly IProfileService _profileService;

    public ResidentsController(IBillService billService, IProfileService profileService)
    {
        _billService = billService;
        _profileService = profileService;
    }

    [HttpGet("bills")]
    public async Task<ActionResult<List<BillDto>>> GetBills()
    {
        var unitIdClaim = User.FindFirst("UnitId")?.Value;
        if (!int.TryParse(unitIdClaim, out var unitId))
        {
            return Unauthorized();
        }

        var bills = await _billService.GetBillsForUnitAsync(unitId);
        return Ok(bills);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var result = await _profileService.GetProfileAsync(residentId);
        return result.Status == ProfileResultStatus.Success
            ? Ok(result.Profile)
            : NotFound(new { message = "Resident not found." });
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var result = await _profileService.UpdateProfileAsync(residentId, request);

        return result.Status switch
        {
            ProfileResultStatus.Success => Ok(new UpdateProfileResponse { Profile = result.Profile! }),
            ProfileResultStatus.EmailTaken => Conflict(new { message = "That email is already in use by another account." }),
            _ => NotFound(new { message = "Resident not found." }),
        };
    }

    [HttpPut("profile/password")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var result = await _profileService.ChangePasswordAsync(residentId, request);

        return result.Status switch
        {
            ChangePasswordResultStatus.Success => Ok(new ChangePasswordResponse()),
            ChangePasswordResultStatus.InvalidCurrentPassword => Unauthorized(new { message = "Current password is incorrect." }),
            _ => NotFound(new { message = "Resident not found." }),
        };
    }

    [HttpPut("profile/notifications")]
    public async Task<ActionResult<NotificationPreferencesResponse>> UpdateNotificationPreferences(
        [FromBody] NotificationPreferencesDto preferences)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var result = await _profileService.UpdateNotificationPreferencesAsync(residentId, preferences);

        return result.Status == ProfileResultStatus.Success
            ? Ok(new NotificationPreferencesResponse { NotificationPreferences = result.Profile!.NotificationPreferences })
            : NotFound(new { message = "Resident not found." });
    }

    private bool TryGetResidentId(out int residentId)
    {
        var residentIdClaim = User.FindFirst("ResidentId")?.Value;
        return int.TryParse(residentIdClaim, out residentId);
    }
}
