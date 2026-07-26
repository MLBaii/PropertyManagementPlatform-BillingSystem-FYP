using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles = "Admin,AdminManager")]
[Route("api/admin/properties")]
public class AdminPropertiesController(AppDbContext context) : ControllerBase
{
    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<AdminUnitDto>>> GetUnits() => Ok(await context.Units.AsNoTracking().OrderBy(unit => unit.UnitNumber).Select(unit => new AdminUnitDto { UnitId = unit.UnitId, UnitNumber = unit.UnitNumber, Floor = unit.Floor, Type = unit.Type, IsActive = unit.IsActive, ResidentCount = unit.Residents.Count, Residents = unit.Residents.OrderBy(resident => resident.Name).Select(resident => new AdminResidentAccountDto { ResidentId = resident.ResidentId, Name = resident.Name, Username = resident.Email, IsActive = resident.IsActive }).ToList() }).ToListAsync());

    [HttpPut("residents/{residentId:int}/password")]
    [Authorize(Roles = "AdminManager")]
    public async Task<IActionResult> ResetResidentPassword(int residentId, [FromBody] ResetResidentPasswordRequest request)
    {
        var resident = await context.Residents.FirstOrDefaultAsync(item => item.ResidentId == residentId);
        if (resident is null) return NotFound(new { message = "Resident account was not found." });

        resident.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog { AdminUserId = adminId, ActionType = "Reset password", AffectedEntity = "Resident", AffectedEntityId = resident.ResidentId, Description = $"Reset password for resident {resident.Email} in unit {resident.UnitId}." });
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("residents")]
    [Authorize(Roles = "AdminManager")]
    public async Task<ActionResult<AdminResidentAccountDto>> RegisterResident([FromBody] RegisterResidentRequest request)
    {
        var unit = await context.Units.FirstOrDefaultAsync(item => item.UnitId == request.UnitId && item.IsActive);
        if (unit is null) return BadRequest(new { message = "Choose an active unit for the resident." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await context.Residents.AnyAsync(item => item.Email == email))
            return Conflict(new { message = "An account with this email address already exists." });

        var resident = new PropertyBill.Api.Models.Resident
        {
            UnitId = unit.UnitId,
            Name = request.Name.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Account = new PropertyBill.Api.Models.Account(),
            IsActive = true
        };
        context.Residents.Add(resident);
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog { AdminUserId = adminId, ActionType = "Register resident", AffectedEntity = "Resident", Description = $"Registered resident {email} for unit {unit.UnitNumber}." });
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUnits), new AdminResidentAccountDto { ResidentId = resident.ResidentId, Name = resident.Name, Username = resident.Email, IsActive = resident.IsActive });
    }

    [HttpPost("units")]
    [Authorize(Roles = "AdminManager")]
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
