using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/billing-items")]
public class AdminBillingItemsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminBillingItemDto>>> Get() => Ok(await context.BillingItems.AsNoTracking().OrderBy(item => item.ChargeType).Select(item => new AdminBillingItemDto { BillingItemId=item.BillingItemId, ChargeType=item.ChargeType, DefaultRate=item.DefaultRate, Frequency=item.Frequency, BillingDay=item.BillingDay, DueDay=item.DueDay, PenaltyRate=item.PenaltyRate, GracePeriodDays=item.GracePeriodDays, IsActive=item.IsActive }).ToListAsync());
    [HttpPost]
    public async Task<ActionResult<AdminBillingItemDto>> Create([FromBody] CreateBillingItemRequest request)
    {
        var property=await context.Properties.FirstOrDefaultAsync(); if(property is null)return BadRequest(new{message="Create a property first."});
        var name=request.ChargeType.Trim();
        var frequency=request.Frequency.Trim();
        var validFrequencies=new[]{"Monthly","Quarterly","Yearly"};
        if(!validFrequencies.Contains(frequency))return BadRequest(new{message="Billing frequency must be Monthly, Quarterly, or Yearly."});
        if(await context.BillingItems.AnyAsync(item=>item.PropertyId==property.PropertyId&&item.ChargeType==name))return Conflict(new{message="A billing item with this charge type already exists."});
        var existingDueDay = await context.BillingItems.Where(item => item.IsActive).Select(item => (int?)item.DueDay).FirstOrDefaultAsync();
        if (existingDueDay.HasValue && existingDueDay.Value != request.DueDay) return BadRequest(new { message = $"Active billing items use due day {existingDueDay}. Use the same due day for one combined bill." });
        var item=new PropertyBill.Api.Models.BillingItem{PropertyId=property.PropertyId,ChargeType=name,DefaultRate=request.DefaultRate,Frequency=frequency,BillingDay=request.BillingDay,DueDay=request.DueDay,PenaltyRate=request.PenaltyRate,GracePeriodDays=request.GracePeriodDays,IsActive=true}; context.BillingItems.Add(item);
        if(int.TryParse(User.FindFirst("AdminUserId")?.Value,out var adminId))context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog{AdminUserId=adminId,ActionType="Create",AffectedEntity="BillingItem",Description=$"Created billing item {name}."});
        await context.SaveChangesAsync(); return CreatedAtAction(nameof(Get),new AdminBillingItemDto{BillingItemId=item.BillingItemId,ChargeType=item.ChargeType,DefaultRate=item.DefaultRate,Frequency=item.Frequency,BillingDay=item.BillingDay,DueDay=item.DueDay,PenaltyRate=item.PenaltyRate,GracePeriodDays=item.GracePeriodDays,IsActive=item.IsActive});
    }

    [HttpPatch("{billingItemId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int billingItemId, [FromBody] UpdateBillingItemStatusRequest request)
    {
        var item = await context.BillingItems.FindAsync(billingItemId);
        if (item is null) return NotFound(new { message = "Billing item was not found." });
        if (request.IsActive)
        {
            var existingDueDay = await context.BillingItems.Where(other => other.IsActive && other.BillingItemId != billingItemId).Select(other => (int?)other.DueDay).FirstOrDefaultAsync();
            if (existingDueDay.HasValue && existingDueDay.Value != item.DueDay) return BadRequest(new { message = $"Active billing items use due day {existingDueDay}. Update this item before reactivating it." });
        }

        item.IsActive = request.IsActive;
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog
            {
                AdminUserId = adminId,
                ActionType = request.IsActive ? "Reactivate" : "Deactivate",
                AffectedEntity = "BillingItem",
                AffectedEntityId = item.BillingItemId,
                Description = $"{(request.IsActive ? "Reactivated" : "Deactivated")} billing item {item.ChargeType}."
            });

        await context.SaveChangesAsync();
        return NoContent();
    }
}
