using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class BillService : IBillService
{
    private readonly IBillRepository _billRepository;

    public BillService(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<List<BillDto>> GetBillsForUnitAsync(int unitId, string? statusFilter)
    {
        var bills = await _billRepository.GetByUnitIdAsync(unitId);
        var dtos = bills.Select(ToDto).ToList();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            dtos = dtos
                .Where(d => string.Equals(d.Status, statusFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return dtos;
    }

    public async Task<BillDetailDto?> GetBillDetailAsync(int unitId, int billId)
    {
        var bill = await _billRepository.GetByIdForUnitAsync(billId, unitId);
        if (bill is null)
        {
            return null;
        }

        return new BillDetailDto
        {
            BillId = bill.BillId,
            ReferenceNumber = bill.ReferenceNumber,
            BillingPeriod = bill.BillingPeriod,
            IssueDate = bill.IssueDate,
            DueDate = bill.DueDate,
            TotalAmount = bill.TotalAmount,
            OutstandingBalance = bill.OutstandingBalance,
            Status = ComputeEffectiveStatus(bill),
            DaysUntilDue = ComputeDaysUntilDue(bill),
            LineItems = bill.BillLineItems
                .Select(li => new BillLineItemDto
                {
                    LineItemId = li.LineItemId,
                    Description = li.Description,
                    Amount = li.Amount,
                    LineItemType = li.LineItemType,
                })
                .ToList(),
            UnitNumber = bill.Unit.UnitNumber,
            PropertyName = bill.Unit.Property.Name,
        };
    }

    private static BillDto ToDto(Bill bill)
    {
        return new BillDto
        {
            BillId = bill.BillId,
            ReferenceNumber = bill.ReferenceNumber,
            BillingPeriod = bill.BillingPeriod,
            IssueDate = bill.IssueDate,
            DueDate = bill.DueDate,
            TotalAmount = bill.TotalAmount,
            OutstandingBalance = bill.OutstandingBalance,
            Status = ComputeEffectiveStatus(bill),
            DaysUntilDue = ComputeDaysUntilDue(bill),
        };
    }

    // "Overdue" isn't a value anyone writes to Bill.Status — it's derived from the due date,
    // since nothing in this project runs a scheduled job to flip a stored status over.
    // A dispute's own status drives a two-stage override that takes priority over every
    // other computed status (including Paid — a resident can dispute a bill they've already
    // paid): "Open" (not yet looked at) shows "Disputed", "UnderReview" (admin has seen it)
    // shows "PendingDispute". Once it's "Resolved" (or "Rejected"), neither check matches and
    // the bill falls through to its normal computed status below — re-evaluated fresh on
    // every call, so no separate bookkeeping is needed to "revert" it.
    private static string ComputeEffectiveStatus(Bill bill)
    {
        if (bill.Disputes.Any(d => d.Status == "Open"))
        {
            return "Disputed";
        }

        if (bill.Disputes.Any(d => d.Status == "UnderReview"))
        {
            return "PendingDispute";
        }

        if (string.Equals(bill.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            return "Paid";
        }

        if (string.Equals(bill.Status, "ProofSubmitted", StringComparison.OrdinalIgnoreCase))
        {
            return "ProofSubmitted";
        }

        return bill.DueDate.Date < DateTime.UtcNow.Date ? "Overdue" : "Unpaid";
    }

    private static int ComputeDaysUntilDue(Bill bill)
    {
        return (bill.DueDate.Date - DateTime.UtcNow.Date).Days;
    }
}
