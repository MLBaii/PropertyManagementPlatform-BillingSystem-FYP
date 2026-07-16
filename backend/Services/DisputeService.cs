using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class DisputeService : IDisputeService
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly IBillRepository _billRepository;

    public DisputeService(IDisputeRepository disputeRepository, IBillRepository billRepository)
    {
        _disputeRepository = disputeRepository;
        _billRepository = billRepository;
    }

    public async Task<DisputeSubmitResult> SubmitAsync(int residentId, int unitId, SubmitDisputeRequest request)
    {
        // Scoped by unitId, same ownership pattern as Bill Detail — a billId belonging to
        // another resident's unit simply doesn't come back, so "doesn't exist" and "isn't
        // yours" are indistinguishable (uniform 404) rather than confirming it exists via 403.
        var bill = await _billRepository.GetByIdForUnitAsync(request.BillId, unitId);
        if (bill is null)
        {
            return DisputeSubmitResult.BillNotFound();
        }

        var active = await _disputeRepository.GetActiveForBillAsync(request.BillId);
        if (active is not null)
        {
            return DisputeSubmitResult.ActiveDisputeExists(ToDto(active));
        }

        var dispute = new Dispute
        {
            BillId = request.BillId,
            ResidentId = residentId,
            Reason = request.Reason,
            Status = "Open",
            SubmittedAt = DateTime.UtcNow,
            // Set explicitly rather than relying on EF's relationship-fixup timing — ToDto
            // needs Bill.ReferenceNumber/BillingPeriod right after CreateAsync returns.
            Bill = bill,
        };

        var created = await _disputeRepository.CreateAsync(dispute);
        return DisputeSubmitResult.Success(ToDto(created));
    }

    public async Task<List<DisputeDto>> GetHistoryAsync(int residentId, string? statusFilter)
    {
        var disputes = await _disputeRepository.GetByResidentIdAsync(residentId, statusFilter);
        return disputes.Select(ToDto).ToList();
    }

    public async Task<DisputeDto?> GetByIdAsync(int disputeId, int residentId)
    {
        var dispute = await _disputeRepository.GetByIdForResidentAsync(disputeId, residentId);
        return dispute is null ? null : ToDto(dispute);
    }

    private static DisputeDto ToDto(Dispute dispute)
    {
        return new DisputeDto
        {
            DisputeId = dispute.DisputeId,
            BillId = dispute.BillId,
            BillReferenceNumber = dispute.Bill.ReferenceNumber,
            BillingPeriod = dispute.Bill.BillingPeriod,
            Reason = dispute.Reason,
            Status = dispute.Status,
            SubmittedAt = dispute.SubmittedAt,
            ResolvedAt = dispute.ResolvedAt,
            AdminResponse = dispute.AdminResponse,
        };
    }
}
