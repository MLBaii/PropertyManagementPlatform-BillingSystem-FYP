using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Services;
namespace PropertyBill.Api.Controllers;
[ApiController]
[Authorize(Roles="Admin")]
[Route("api/admin/reminders")]
public class AdminRemindersController(AppDbContext context, INotificationSendingService notifications):ControllerBase
{
    [HttpGet("overdue")]
    public async Task<ActionResult<IEnumerable<OverdueReminderDto>>> GetOverdue()
    {
        var today=DateTime.UtcNow.Date;
        var bills = await context.Bills.Include(bill => bill.Unit).ThenInclude(unit => unit.Residents).Where(bill => bill.Status != "Paid" && bill.OutstandingBalance > 0 && bill.DueDate < today).ToListAsync();
        var notifications = await context.Notifications.Where(notification => notification.Type == "DueReminder").ToListAsync();
        return Ok(bills.SelectMany(bill => bill.Unit.Residents.Select(resident => new OverdueReminderDto { ResidentId = resident.ResidentId, ResidentName = resident.Name, UnitNumber = bill.Unit.UnitNumber, BillReference = bill.ReferenceNumber, OutstandingAmount = bill.OutstandingBalance, DaysOverdue = (today - bill.DueDate.Date).Days, ReminderSent = notifications.Any(notification => notification.ResidentId == resident.ResidentId && notification.DeepLink == $"/(tabs)/bills/{bill.BillId}") } )));
    }
    [HttpPost("overdue")]
    public async Task<ActionResult<SendOverdueRemindersResponse>> SendOverdue()
    {
        var bills=await context.Bills.Include(bill=>bill.Unit).ThenInclude(unit=>unit.Residents).Where(bill=>bill.Status!="Paid"&&bill.OutstandingBalance>0&&bill.DueDate<DateTime.UtcNow.Date).ToListAsync(); var sent=0;
        foreach(var bill in bills) foreach(var resident in bill.Unit.Residents){await notifications.SendAsync(resident.ResidentId,"DueReminder","Payment overdue",$"Your bill {bill.ReferenceNumber} has an outstanding balance of RM {bill.OutstandingBalance:N2}.",$"/(tabs)/bills/{bill.BillId}");sent++;}
        return Ok(new SendOverdueRemindersResponse{RemindersSent=sent});
    }
}
