using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles="Admin")]
[Route("api/admin/reports")]
public class AdminReportsController(AppDbContext context):ControllerBase
{
    [HttpGet("aging-summary")]
    public async Task<ActionResult<AdminReportSummaryDto>> AgingSummary()
    {
        var bills=await context.Bills.AsNoTracking().Where(bill=>bill.Status!="Paid"&&bill.OutstandingBalance>0).ToListAsync(); var today=DateTime.UtcNow.Date;
        decimal Sum(Func<int,bool> predicate)=>bills.Where(bill=>predicate((today-bill.DueDate.Date).Days)).Sum(bill=>bill.OutstandingBalance);
        var summary=new AdminReportSummaryDto{Current=Sum(days=>days<=0),Days1To30=Sum(days=>days>=1&&days<=30),Days31To60=Sum(days=>days>=31&&days<=60),Days61To90=Sum(days=>days>=61&&days<=90),Over90Days=Sum(days=>days>90)};summary.TotalOutstanding=summary.Current+summary.Days1To30+summary.Days31To60+summary.Days61To90+summary.Over90Days;return Ok(summary);
    }
}
