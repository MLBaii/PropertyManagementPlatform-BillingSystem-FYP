using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/bills")]
public class AdminBillsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminBillDto>>> Get() => Ok(await context.Bills.AsNoTracking().Include(bill => bill.Unit).OrderByDescending(bill => bill.DueDate).Select(bill => new AdminBillDto { BillId=bill.BillId,ReferenceNumber=bill.ReferenceNumber,UnitNumber=bill.Unit.UnitNumber,BillingPeriod=bill.BillingPeriod,DueDate=bill.DueDate,TotalAmount=bill.TotalAmount,OutstandingBalance=bill.OutstandingBalance,Status=bill.Status=="Paid"||bill.Status=="Voided"?bill.Status:bill.DueDate.Date<DateTime.UtcNow.Date?"Overdue":bill.Status }).ToListAsync());

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateBillsResponse>> Generate([FromBody] GenerateBillsRequest request)
    {
        if (!DateTime.TryParseExact(request.BillingPeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var billingMonth))
            return BadRequest(new { message = "Billing period must use YYYY-MM format." });
        if (billingMonth.Year < 2020 || billingMonth.Year > 2100)
            return BadRequest(new { message = "Billing period must be between January 2020 and December 2100." });
        if (request.DueDate == default || request.DueDate.Year != billingMonth.Year || request.DueDate.Month != billingMonth.Month)
            return BadRequest(new { message = "Due date must be within the selected billing period." });
        if (await context.Bills.AnyAsync(bill => bill.BillingPeriod == request.BillingPeriod)) return Conflict(new { message = "Bills have already been generated for this period." });
        var units = await context.Units.Where(unit => unit.IsActive).ToListAsync(); var items = await context.BillingItems.Where(item => item.IsActive).ToListAsync();
        if (units.Count == 0) return BadRequest(new { message = "At least one active unit is required before bills can be generated." });
        if (items.Count == 0) return BadRequest(new { message = "At least one active billing item is required before bills can be generated." });
        var additionalCharges = request.IncludeAdditionalCharges
            ? await context.AdditionalCharges.Where(charge => charge.Status == "Pending" && charge.BillingPeriod == request.BillingPeriod).ToListAsync()
            : [];
        var dueDateUtc = DateTime.SpecifyKind(request.DueDate.Date, DateTimeKind.Utc);
        var generated = new List<(PropertyBill.Api.Models.Bill Bill, List<PropertyBill.Api.Models.AdditionalCharge> Charges)>();
        foreach (var unit in units)
        {
            var unitCharges = additionalCharges.Where(charge => charge.UnitId == unit.UnitId).ToList();
            var total = items.Sum(item => item.DefaultRate) + unitCharges.Sum(charge => charge.Amount);
            var bill = new PropertyBill.Api.Models.Bill { UnitId=unit.UnitId,BillingPeriod=request.BillingPeriod,ReferenceNumber=$"BILL-{request.BillingPeriod}-{unit.UnitNumber.Replace("-","")}",IssueDate=DateTime.UtcNow,DueDate=dueDateUtc,Status="Unpaid",TotalAmount=total,OutstandingBalance=total };
            foreach(var item in items) bill.BillLineItems.Add(new PropertyBill.Api.Models.BillLineItem { Description=item.ChargeType,Amount=item.DefaultRate,LineItemType="Charge",BillingItemId=item.BillingItemId });
            foreach (var charge in unitCharges) bill.BillLineItems.Add(new PropertyBill.Api.Models.BillLineItem { Description=$"{charge.Category}: {charge.Description}", Amount=charge.Amount, LineItemType="AdditionalCharge" });
            context.Bills.Add(bill); generated.Add((bill, unitCharges));
        }
        await context.SaveChangesAsync();
        foreach (var (bill, charges) in generated) foreach (var charge in charges) { charge.Status = "Billed"; charge.BillId = bill.BillId; }
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
