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
            BillingPeriodStart = b.BillingPeriodStart,
            BillingPeriodEnd = b.BillingPeriodEnd,
            DueDate = b.DueDate,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
        }).ToList();
    }
}
