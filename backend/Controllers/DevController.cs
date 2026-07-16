using Microsoft.AspNetCore.Mvc;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;

namespace PropertyBill.Api.Controllers;

// Not [Authorize] — this is a development-only tool for triggering a notification without
// the (not-yet-built) admin module. The controller is mapped unconditionally by
// app.MapControllers() (like every other controller), so the IsDevelopment() check inside
// the action below is the only thing standing between this and running in Production —
// keep it, don't rely on "well it's a dev tool" alone.
[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private static readonly Dictionary<string, (string Title, string Body)> SampleContent = new()
    {
        ["BillIssued"] = ("New bill issued", "A new bill has been issued for your unit. Tap to view details."),
        ["BillOverdue"] = ("Bill overdue", "Please make payment as soon as possible."),
        ["PaymentConfirmed"] = ("Payment confirmed", "Your payment has been confirmed. Thank you!"),
        ["DueReminder"] = ("Due date approaching", "Your bill is due soon."),
    };

    private readonly INotificationSendingService _notificationSendingService;
    private readonly IWebHostEnvironment _environment;

    public DevController(INotificationSendingService notificationSendingService, IWebHostEnvironment environment)
    {
        _notificationSendingService = notificationSendingService;
        _environment = environment;
    }

    [HttpPost("notifications/test")]
    public async Task<IActionResult> SendTestNotification([FromBody] DevNotificationTestRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var (sampleTitle, sampleBody) = SampleContent.TryGetValue(request.Type, out var sample)
            ? sample
            : SampleContent["BillIssued"];

        var sent = await _notificationSendingService.SendAsync(
            request.ResidentId,
            request.Type,
            request.Title ?? sampleTitle,
            request.Body ?? sampleBody,
            request.DeepLink);

        return sent ? Ok(new { message = "Notification sent." }) : NotFound(new { message = "Resident not found." });
    }
}
