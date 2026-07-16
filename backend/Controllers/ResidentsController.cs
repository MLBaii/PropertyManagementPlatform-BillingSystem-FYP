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
    private readonly INotificationTokenService _notificationTokenService;
    private readonly INotificationService _notificationService;
    private readonly IDisputeService _disputeService;

    public ResidentsController(
        IBillService billService,
        IProfileService profileService,
        IDashboardService dashboardService,
        IPaymentProofService paymentProofService,
        INotificationTokenService notificationTokenService,
        INotificationService notificationService,
        IDisputeService disputeService)
    {
        _billService = billService;
        _profileService = profileService;
        _dashboardService = dashboardService;
        _paymentProofService = paymentProofService;
        _notificationTokenService = notificationTokenService;
        _notificationService = notificationService;
        _disputeService = disputeService;
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

    [HttpPost("notification-tokens")]
    public async Task<ActionResult<NotificationTokenDto>> RegisterNotificationToken(
        [FromBody] RegisterNotificationTokenRequest request)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var token = await _notificationTokenService.RegisterAsync(residentId, request);
        return Ok(token);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var notifications = await _notificationService.GetHistoryAsync(residentId);
        return Ok(notifications);
    }

    [HttpPut("notifications/{id:int}/read")]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var success = await _notificationService.MarkAsReadAsync(id, residentId);
        return success ? NoContent() : NotFound(new { message = "Notification not found." });
    }

    [HttpPut("notifications/read-all")]
    public async Task<IActionResult> MarkAllNotificationsRead()
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        await _notificationService.MarkAllAsReadAsync(residentId);
        return NoContent();
    }

    [HttpPost("disputes")]
    public async Task<ActionResult<DisputeDto>> SubmitDispute([FromBody] SubmitDisputeRequest request)
    {
        if (!TryGetResidentId(out var residentId) || !TryGetUnitId(out var unitId))
        {
            return Unauthorized();
        }

        var result = await _disputeService.SubmitAsync(residentId, unitId, request);

        return result.Status switch
        {
            DisputeSubmitStatus.Success => StatusCode(StatusCodes.Status201Created, result.Dispute),
            DisputeSubmitStatus.BillNotFound => NotFound(new { message = result.ErrorMessage }),
            DisputeSubmitStatus.ActiveDisputeExists => Conflict(new { message = result.ErrorMessage, dispute = result.Dispute }),
            _ => BadRequest(new { message = result.ErrorMessage }),
        };
    }

    [HttpGet("disputes")]
    public async Task<ActionResult<List<DisputeDto>>> GetDisputes([FromQuery] string? status)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var disputes = await _disputeService.GetHistoryAsync(residentId, status);
        return Ok(disputes);
    }

    [HttpGet("disputes/{id:int}")]
    public async Task<ActionResult<DisputeDto>> GetDisputeDetail(int id)
    {
        if (!TryGetResidentId(out var residentId))
        {
            return Unauthorized();
        }

        var dispute = await _disputeService.GetByIdAsync(id, residentId);
        return dispute is null ? NotFound(new { message = "Dispute not found." }) : Ok(dispute);
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
