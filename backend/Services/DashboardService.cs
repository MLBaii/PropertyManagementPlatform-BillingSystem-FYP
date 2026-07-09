using System.Globalization;
using PropertyBill.Api.Dtos;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly IResidentRepository _residentRepository;
    private readonly IBillRepository _billRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DashboardService(
        IResidentRepository residentRepository,
        IBillRepository billRepository,
        IPaymentRepository paymentRepository)
    {
        _residentRepository = residentRepository;
        _billRepository = billRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<DashboardDto?> GetDashboardAsync(int residentId, int unitId)
    {
        var resident = await _residentRepository.GetByIdAsync(residentId);
        if (resident is null)
        {
            return null;
        }

        var bills = await _billRepository.GetByUnitIdAsync(unitId);
        var payments = await _paymentRepository.GetConfirmedByUnitIdAsync(unitId);

        var billActivity = bills.Select(b => new ActivityDto
        {
            Type = "BillIssued",
            Date = b.IssueDate,
            Description = $"Bill issued · {FormatBillingPeriod(b.BillingPeriod)}",
            Reference = b.ReferenceNumber,
            Amount = b.TotalAmount,
        });

        var paymentActivity = payments.Select(p => new ActivityDto
        {
            Type = "PaymentConfirmed",
            Date = p.PaymentDate,
            Description = "Payment confirmed",
            Reference = $"{FormatBillingPeriod(p.Bill.BillingPeriod)} bill",
            Amount = p.Amount,
        });

        var recentActivity = billActivity
            .Concat(paymentActivity)
            .OrderByDescending(a => a.Date)
            .Take(3)
            .ToList();

        return new DashboardDto
        {
            UnitNumber = resident.Unit.UnitNumber,
            PropertyName = resident.Unit.Property.Name,
            TotalOutstanding = bills.Sum(b => b.OutstandingBalance),
            TotalPaid = payments.Sum(p => p.Amount),
            CreditBalance = resident.Account.CreditBalance,
            RecentActivity = recentActivity,
        };
    }

    // "2026-06" -> "June 2026", matching the frontend's formatBillingPeriod (formatDate.ts)
    // so activity descriptions read the same whichever side happens to render them.
    private static string FormatBillingPeriod(string billingPeriod)
    {
        if (DateTime.TryParseExact(
                $"{billingPeriod}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        }

        return billingPeriod;
    }
}
