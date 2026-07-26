using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,AdminManager")]
[Route("api/admin/disputes")]
public class AdminDisputesController(AppDbContext context, INotificationSendingService notificationSending) : ControllerBase
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

    [HttpGet("{disputeId:int}/bill")]
    public async Task<ActionResult<DisputeBillCorrectionDto>> GetRelatedBill(int disputeId)
    {
        var dispute = await context.Disputes.AsNoTracking()
            .Include(item => item.Bill).ThenInclude(bill => bill.BillLineItems)
            .Include(item => item.Bill).ThenInclude(bill => bill.Payments)
            .FirstOrDefaultAsync(item => item.DisputeId == disputeId);
        if (dispute is null) return NotFound(new { message = "Dispute was not found." });

        var bill = dispute.Bill;
        var canAdjust = dispute.Status is not ("Resolved" or "Rejected") && CanAdjustBill(bill);
        return Ok(new DisputeBillCorrectionDto
        {
            BillId = bill.BillId,
            ReferenceNumber = bill.ReferenceNumber,
            BillingPeriod = bill.BillingPeriod,
            Status = bill.Status,
            TotalAmount = bill.TotalAmount,
            OutstandingBalance = bill.OutstandingBalance,
            CanAdjust = canAdjust,
            AdjustmentBlockedReason = canAdjust ? null : "Closed disputes and bills with confirmed payments or a closed status cannot be changed.",
            LineItems = bill.BillLineItems.OrderBy(line => line.LineItemId).Select(line => new DisputeBillLineItemDto
            {
                LineItemId = line.LineItemId,
                Description = line.Description,
                Amount = line.Amount,
                LineItemType = line.LineItemType
            }).ToList()
        });
    }

    [HttpPut("{disputeId:int}/bill-lines/{lineItemId:int}")]
    public async Task<IActionResult> UpdateRelatedBillLine(int disputeId, int lineItemId, [FromBody] UpdateDisputeBillLineItemRequest request)
    {
        var dispute = await context.Disputes.Include(item => item.Bill).ThenInclude(bill => bill.Payments)
            .FirstOrDefaultAsync(item => item.DisputeId == disputeId);
        if (dispute is null) return NotFound(new { message = "Dispute was not found." });
        if (dispute.Status is "Resolved" or "Rejected") return Conflict(new { message = "A closed dispute cannot be used to change a bill." });
        if (!CanAdjustBill(dispute.Bill)) return Conflict(new { message = "This bill can no longer be adjusted because it has a confirmed payment or closed status." });
        var line = await context.BillLineItems.FirstOrDefaultAsync(item => item.LineItemId == lineItemId && item.BillId == dispute.BillId);
        if (line is null) return NotFound(new { message = "Bill line item was not found." });

        var previousDescription = line.Description;
        var previousAmount = line.Amount;
        line.Description = request.Description.Trim();
        line.Amount = request.Amount;
        RecalculateBill(dispute.Bill, previousAmount - line.Amount);
        AddAudit("Adjust bill line", dispute, $"Adjusted {dispute.Bill.ReferenceNumber} line '{previousDescription}' from RM {previousAmount:0.00} to RM {line.Amount:0.00}.");
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{disputeId:int}/bill-lines/{lineItemId:int}")]
    public async Task<IActionResult> DeleteRelatedBillLine(int disputeId, int lineItemId)
    {
        var dispute = await context.Disputes.Include(item => item.Bill).ThenInclude(bill => bill.Payments)
            .Include(item => item.Bill).ThenInclude(bill => bill.BillLineItems)
            .FirstOrDefaultAsync(item => item.DisputeId == disputeId);
        if (dispute is null) return NotFound(new { message = "Dispute was not found." });
        if (dispute.Status is "Resolved" or "Rejected") return Conflict(new { message = "A closed dispute cannot be used to change a bill." });
        if (!CanAdjustBill(dispute.Bill)) return Conflict(new { message = "This bill can no longer be adjusted because it has a confirmed payment or closed status." });
        var line = dispute.Bill.BillLineItems.FirstOrDefault(item => item.LineItemId == lineItemId);
        if (line is null) return NotFound(new { message = "Bill line item was not found." });

        context.BillLineItems.Remove(line);
        RecalculateBill(dispute.Bill, line.Amount);
        AddAudit("Remove bill line", dispute, $"Removed '{line.Description}' (RM {line.Amount:0.00}) from bill {dispute.Bill.ReferenceNumber} while reviewing a dispute.");
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{disputeId:int}/bill-lines")]
    public async Task<ActionResult<DisputeBillLineItemDto>> AddRelatedBillLine(int disputeId, [FromBody] AddDisputeBillLineItemRequest request)
    {
        var dispute = await context.Disputes.Include(item => item.Bill).ThenInclude(bill => bill.Payments)
            .FirstOrDefaultAsync(item => item.DisputeId == disputeId);
        if (dispute is null) return NotFound(new { message = "Dispute was not found." });
        if (dispute.Status is "Resolved" or "Rejected") return Conflict(new { message = "A closed dispute cannot be used to change a bill." });
        if (!CanAdjustBill(dispute.Bill)) return Conflict(new { message = "This bill can no longer be adjusted because it has a confirmed payment or closed status." });

        var line = new PropertyBill.Api.Models.BillLineItem
        {
            BillId = dispute.BillId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            LineItemType = "Additional charge"
        };
        context.BillLineItems.Add(line);
        RecalculateBill(dispute.Bill, -line.Amount);
        AddAudit("Add bill line", dispute, $"Added '{line.Description}' (RM {line.Amount:0.00}) to bill {dispute.Bill.ReferenceNumber} while reviewing a dispute.");
        await context.SaveChangesAsync();
        return Ok(new DisputeBillLineItemDto { LineItemId = line.LineItemId, Description = line.Description, Amount = line.Amount, LineItemType = line.LineItemType });
    }

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
        AddAudit("Review", dispute, $"Dispute for bill {dispute.Bill.ReferenceNumber} marked {request.Status}.");

        await context.SaveChangesAsync();
        if (request.Status is "Resolved" or "Rejected")
        {
            var title = request.Status == "Resolved" ? "Dispute resolved" : "Dispute reviewed";
            var body = request.Status == "Resolved"
                ? $"Your dispute for bill {dispute.Bill.ReferenceNumber} has been resolved. {dispute.AdminResponse}"
                : $"Your dispute for bill {dispute.Bill.ReferenceNumber} has been reviewed. {dispute.AdminResponse}";
            await notificationSending.SendAsync(dispute.ResidentId, "DisputeResolved", title, body, "/(tabs)/disputes");
        }
        return NoContent();
    }

    private static bool CanAdjustBill(PropertyBill.Api.Models.Bill bill) =>
        bill.Status is not ("Paid" or "Voided") && !bill.Payments.Any(payment => payment.Status == "Confirmed");

    private static void RecalculateBill(PropertyBill.Api.Models.Bill bill, decimal adjustment = 0)
    {
        bill.TotalAmount = Math.Max(0, bill.TotalAmount - adjustment);
        bill.OutstandingBalance = bill.TotalAmount;
    }

    private void AddAudit(string actionType, PropertyBill.Api.Models.Dispute dispute, string description)
    {
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog
            {
                AdminUserId = adminId,
                ActionType = actionType,
                AffectedEntity = "Dispute",
                AffectedEntityId = dispute.DisputeId,
                Description = description
            });
    }
}
