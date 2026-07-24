using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/bills")]
public class AdminBillsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminBillDto>>> Get() => Ok(await context.Bills.AsNoTracking().Include(bill => bill.Unit).OrderByDescending(bill => bill.DueDate).Select(bill => new AdminBillDto { BillId=bill.BillId,ReferenceNumber=bill.ReferenceNumber,UnitNumber=bill.Unit.UnitNumber,BillingPeriod=bill.BillingPeriod,DueDate=bill.DueDate,TotalAmount=bill.TotalAmount,OutstandingBalance=bill.OutstandingBalance,Status=bill.Status=="Paid"?"Paid":bill.DueDate.Date<DateTime.UtcNow.Date?"Overdue":bill.Status }).ToListAsync());

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateBillsResponse>> Generate([FromBody] GenerateBillsRequest request)
    {
        if (await context.Bills.AnyAsync(bill => bill.BillingPeriod == request.BillingPeriod)) return Conflict(new { message = "Bills have already been generated for this period." });
        var units = await context.Units.Where(unit => unit.IsActive).ToListAsync(); var items = await context.BillingItems.Where(item => item.IsActive).ToListAsync();
        var dueDateUtc = DateTime.SpecifyKind(request.DueDate.Date, DateTimeKind.Utc);
        foreach (var unit in units) { var bill = new PropertyBill.Api.Models.Bill { UnitId=unit.UnitId,BillingPeriod=request.BillingPeriod,ReferenceNumber=$"BILL-{request.BillingPeriod}-{unit.UnitNumber.Replace("-","")}",IssueDate=DateTime.UtcNow,DueDate=dueDateUtc,Status="Unpaid",TotalAmount=items.Sum(item=>item.DefaultRate),OutstandingBalance=items.Sum(item=>item.DefaultRate) }; foreach(var item in items) bill.BillLineItems.Add(new PropertyBill.Api.Models.BillLineItem { Description=item.ChargeType,Amount=item.DefaultRate,LineItemType="Charge",BillingItemId=item.BillingItemId }); context.Bills.Add(bill); }
        await context.SaveChangesAsync(); return Ok(new GenerateBillsResponse { BillsGenerated = units.Count });
    }

    [HttpPost("{billId:int}/void")]
    public async Task<IActionResult> Void(int billId)
    {
        var bill = await context.Bills.Include(item => item.Payments).FirstOrDefaultAsync(item => item.BillId == billId);
        if (bill is null) return NotFound(new { message = "Bill was not found." });
        if (bill.Status == "Voided") return BadRequest(new { message = "This bill has already been voided." });
        if (bill.Status == "Paid" || bill.Payments.Any()) return Conflict(new { message = "A bill with recorded payments cannot be voided." });
        if (bill.Status == "ProofSubmitted") return Conflict(new { message = "A bill with a payment proof under review cannot be voided." });

        bill.Status = "Voided";
        bill.OutstandingBalance = 0;
        if (int.TryParse(User.FindFirst("AdminUserId")?.Value, out var adminId))
            context.AuditLogs.Add(new PropertyBill.Api.Models.AuditLog
            {
                AdminUserId = adminId,
                ActionType = "Void",
                AffectedEntity = "Bill",
                AffectedEntityId = bill.BillId,
                Description = $"Voided bill {bill.ReferenceNumber}."
            });

        await context.SaveChangesAsync();
        return NoContent();
    }
}
