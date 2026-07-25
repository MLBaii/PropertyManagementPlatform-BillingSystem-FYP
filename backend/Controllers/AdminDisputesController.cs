using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/disputes")]
public class AdminDisputesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminDisputeDto>>> Get() => Ok(await context.Disputes.AsNoTracking()
        .Include(dispute => dispute.Resident).ThenInclude(resident => resident.Unit)
        .Include(dispute => dispute.Bill)
        .OrderByDescending(dispute => dispute.SubmittedAt)
        .Select(dispute => new AdminDisputeDto
        {
            DisputeId = dispute.DisputeId,
            ResidentName = dispute.Resident.Name,
            UnitNumber = dispute.Resident.Unit.UnitNumber,
            BillReferenceNumber = dispute.Bill.ReferenceNumber,
            BillingPeriod = dispute.Bill.BillingPeriod,
            TotalAmount = dispute.Bill.TotalAmount,
            OutstandingBalance = dispute.Bill.OutstandingBalance,
            Reason = dispute.Reason,
            Status = dispute.Status,
            SubmittedAt = dispute.SubmittedAt,
            ResolvedAt = dispute.ResolvedAt,
            AdminResponse = dispute.AdminResponse
        }).ToListAsync());

    [HttpPut("{disputeId:int}/review")]
    public async Task<IActionResult> Review(int disputeId, [FromBody] ReviewAdminDisputeRequest request)
    {
        var dispute = await context.Disputes.Include(item => item.Bill).FirstOrDefaultAsync(item => item.DisputeId == disputeId);
        if (dispute is null) return NotFound(new { message = "Dispute was not found." });
        if (dispute.Status is "Resolved" or "Rejected") return Conflict(new { message = "This dispute has already been closed." });
        if (request.Status is "Resolved" or "Rejected" && string.IsNullOrWhiteSpace(request.AdminResponse))
            return BadRequest(new { message = "Add an admin response before closing a dispute." });

        dispute.Status = request.Status;
        dispute.AdminResponse = string.IsNullOrWhiteSpace(request.AdminResponse) ? null : request.AdminResponse.Trim();
        dispute.ResolvedAt = request.Status is "Resolved" or "Rejected" ? DateTime.UtcNow : null;
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog
            {
                AdminUserId = adminId,
                ActionType = "Review",
                AffectedEntity = "Dispute",
                AffectedEntityId = dispute.DisputeId,
                Description = $"Dispute for bill {dispute.Bill.ReferenceNumber} marked {request.Status}."
            });

        await context.SaveChangesAsync();
        return NoContent();
    }
}
