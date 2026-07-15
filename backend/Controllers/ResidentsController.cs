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
    private readonly IDashboardService _dashboardService;
    private readonly IPaymentProofService _paymentProofService;

    public ResidentsController(
        IBillService billService,
        IProfileService profileService,
        IDashboardService dashboardService,
        IPaymentProofService paymentProofService)
    {
        _billService = billService;
        _profileService = profileService;
        _dashboardService = dashboardService;
        _paymentProofService = paymentProofService;
    }

    [HttpPost("payment-proofs")]
    [RequestSizeLimit(16 * 1024 * 1024)] // up to 3 files @ 5MB each + multipart overhead
    public async Task<ActionResult<PaymentProofDto>> SubmitPaymentProof([FromForm] SubmitPaymentProofRequest request)
    {
        if (!TryGetResidentId(out var residentId) || !TryGetUnitId(out var unitId))
        {
            return Unauthorized();
        }

        var result = await _paymentProofService.SubmitAsync(residentId, unitId, request.Files, request.BillIds);

        return result.Status switch
        {
            PaymentProofSubmitStatus.Success => StatusCode(StatusCodes.Status201Created, result.Proof),
            PaymentProofSubmitStatus.StorageUploadFailed => StatusCode(StatusCodes.Status502BadGateway, new { message = result.ErrorMessage }),
            _ => BadRequest(new { message = result.ErrorMessage }),
        };
    }

    [HttpGet("payment-proofs")]
    public async Task<ActionResult<List<PaymentProofDto>>> GetPaymentProofs()
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var history = await _paymentProofService.GetHistoryAsync(residentId);
        return Ok(history);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        if (!TryGetResidentId(out var residentId) || !TryGetUnitId(out var unitId))
        {
            return Unauthorized();
        }

        var dashboard = await _dashboardService.GetDashboardAsync(residentId, unitId);
        return dashboard is null ? NotFound(new { message = "Resident not found." }) : Ok(dashboard);
    }

    [HttpGet("bills")]
    public async Task<ActionResult<List<BillDto>>> GetBills([FromQuery] string? status)
    {
        if (!TryGetUnitId(out var unitId))
        {
            return Unauthorized();
        }

        var bills = await _billService.GetBillsForUnitAsync(unitId, status);
        return Ok(bills);
    }

    [HttpGet("bills/{billId:int}")]
    public async Task<ActionResult<BillDetailDto>> GetBillDetail(int billId)
    {
        if (!TryGetUnitId(out var unitId))
        {
            return Unauthorized();
        }

        var detail = await _billService.GetBillDetailAsync(unitId, billId);
        return detail is null ? NotFound(new { message = "Bill not found." }) : Ok(detail);
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

    private bool TryGetUnitId(out int unitId)
    {
        var unitIdClaim = User.FindFirst("UnitId")?.Value;
        return int.TryParse(unitIdClaim, out unitId);
    }
}
