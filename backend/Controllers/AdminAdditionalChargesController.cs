using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,AdminManager")]
[Route("api/admin/additional-charges")]
public class AdminAdditionalChargesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminAdditionalChargeDto>>> Get()
    {
        var charges = await context.AdditionalCharges.AsNoTracking().Include(charge => charge.Unit)
            .OrderByDescending(charge => charge.CreatedAt).ToListAsync();
        return Ok(charges.Select(charge => ToDto(charge)));
    }

    [HttpPost]
    public async Task<ActionResult<AdminAdditionalChargeDto>> Create([FromBody] CreateAdditionalChargeRequest request)
    {
        if (!DateTime.TryParseExact(request.BillingPeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month) || month.Year is < 2020 or > 2100)
            return BadRequest(new { message = "Billing period must be between January 2020 and December 2100." });
        var unit = await context.Units.FindAsync(request.UnitId);
        if (unit is null || !unit.IsActive) return BadRequest(new { message = "Choose an active unit." });

        var category = request.Category.Trim();
        var charge = new AdditionalCharge
        {
            UnitId = unit.UnitId, BillingPeriod = request.BillingPeriod, Category = category,
            Description = string.IsNullOrWhiteSpace(request.Description) ? category : request.Description.Trim(),
            Amount = request.Amount, Status = "Pending"
        };
        context.AdditionalCharges.Add(charge);
        AddAudit("Create", charge, $"Added {category} additional charge for unit {unit.UnitNumber} ({request.BillingPeriod}).");
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), ToDto(charge, unit.UnitNumber));
    }

    [HttpPost("{additionalChargeId:int}/cancel")]
    public async Task<IActionResult> Cancel(int additionalChargeId)
    {
        var charge = await context.AdditionalCharges.Include(item => item.Unit).FirstOrDefaultAsync(item => item.AdditionalChargeId == additionalChargeId);
        if (charge is null) return NotFound(new { message = "Additional charge was not found." });
        if (charge.Status != "Pending") return Conflict(new { message = "Only pending additional charges can be cancelled." });
        charge.Status = "Cancelled";
        AddAudit("Cancel", charge, $"Cancelled {charge.Category} additional charge for unit {charge.Unit.UnitNumber}.");
        await context.SaveChangesAsync();
        return NoContent();
    }

    private void AddAudit(string action, AdditionalCharge charge, string description)
    {
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new AuditLog { AdminUserId = adminId, ActionType = action, AffectedEntity = "AdditionalCharge", AffectedEntityId = charge.AdditionalChargeId, Description = description });
    }
    private static AdminAdditionalChargeDto ToDto(AdditionalCharge charge, string? unitNumber = null) => new()
    {
        AdditionalChargeId = charge.AdditionalChargeId, UnitId = charge.UnitId, UnitNumber = unitNumber ?? charge.Unit.UnitNumber,
        BillingPeriod = charge.BillingPeriod, Category = charge.Category, Description = charge.Description, Amount = charge.Amount,
        Source = charge.Source, Status = charge.Status, CreatedAt = charge.CreatedAt
    };
}
