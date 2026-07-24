using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/properties")]
public class AdminPropertiesController(AppDbContext context) : ControllerBase
{
    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<AdminUnitDto>>> GetUnits() => Ok(await context.Units.AsNoTracking().OrderBy(unit => unit.UnitNumber).Select(unit => new AdminUnitDto { UnitId = unit.UnitId, UnitNumber = unit.UnitNumber, Floor = unit.Floor, Type = unit.Type, IsActive = unit.IsActive, ResidentCount = unit.Residents.Count }).ToListAsync());

    [HttpPost("units")]
    public async Task<ActionResult<AdminUnitDto>> CreateUnit([FromBody] CreateAdminUnitRequest request)
    {
        var property = await context.Properties.FirstOrDefaultAsync();
        if (property is null) return BadRequest(new { message = "Create a property before adding units." });

        var unitNumber = request.UnitNumber.Trim();
        if (await context.Units.AnyAsync(unit => unit.PropertyId == property.PropertyId && unit.UnitNumber == unitNumber))
            return Conflict(new { message = "This unit number already exists for the property." });

        var unit = new PropertyBill.Api.Models.Unit { PropertyId = property.PropertyId, UnitNumber = unitNumber, Floor = request.Floor, Type = request.Type.Trim(), IsActive = true };
        context.Units.Add(unit);
        var adminId = int.TryParse(User.FindFirst("AdminUserId")?.Value, out var parsedAdminId) ? parsedAdminId : (int?)null;
        if (adminId is not null) context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog { AdminUserId = adminId.Value, ActionType = "Create", AffectedEntity = "Unit", Description = $"Created unit {unitNumber}." });
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUnits), new AdminUnitDto { UnitId = unit.UnitId, UnitNumber = unit.UnitNumber, Floor = unit.Floor, Type = unit.Type, IsActive = unit.IsActive });
    }
}
