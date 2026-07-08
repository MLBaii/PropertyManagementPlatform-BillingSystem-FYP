using PropertyBill.Api.Dtos;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class BillService : IBillService
{
    private readonly IBillRepository _billRepository;

    public BillService(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<List<BillDto>> GetBillsForUnitAsync(int unitId)
    {
        var bills = await _billRepository.GetByUnitIdAsync(unitId);

        return bills.Select(b => new BillDto
        {
            BillId = b.BillId,
            ReferenceNumber = b.ReferenceNumber,
            BillingPeriod = b.BillingPeriod,
            IssueDate = b.IssueDate,
            DueDate = b.DueDate,
            TotalAmount = b.TotalAmount,
            OutstandingBalance = b.OutstandingBalance,
            Status = b.Status,
        }).ToList();
    }
}
