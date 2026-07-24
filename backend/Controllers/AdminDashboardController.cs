using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyBill.Api.Data;
using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public class AdminDashboardController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AdminDashboardDto>> Get()
    {
        var property = await context.Properties.AsNoTracking().FirstOrDefaultAsync();
        var confirmedPayments = await context.Payments.AsNoTracking()
            .Where(payment => payment.Status == "Confirmed")
            .SumAsync(payment => (decimal?)payment.Amount) ?? 0m;

        return Ok(new AdminDashboardDto
        {
            PropertyName = property?.Name ?? "PropertyBill",
            ActiveUnits = await context.Units.AsNoTracking().CountAsync(unit => unit.IsActive),
            OutstandingBalance = await context.Bills.AsNoTracking()
                .Where(bill => bill.Status != "Paid")
                .SumAsync(bill => (decimal?)bill.OutstandingBalance) ?? 0m,
            ConfirmedPayments = confirmedPayments,
            PendingPaymentProofs = await context.PaymentProofs.AsNoTracking()
                .CountAsync(proof => proof.Status == "Pending"),
            OpenDisputes = await context.Disputes.AsNoTracking()
                .CountAsync(dispute => dispute.Status == "Open" || dispute.Status == "UnderReview"),
        });
    }
}
