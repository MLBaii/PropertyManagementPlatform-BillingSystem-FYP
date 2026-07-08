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

    public ResidentsController(IBillService billService)
    {
        _billService = billService;
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
}
