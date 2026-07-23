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
}
