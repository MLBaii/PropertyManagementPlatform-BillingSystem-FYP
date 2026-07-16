using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IDisputeService
{
    Task<DisputeSubmitResult> SubmitAsync(int residentId, int unitId, SubmitDisputeRequest request);
    Task<List<DisputeDto>> GetHistoryAsync(int residentId, string? statusFilter);
    Task<DisputeDto?> GetByIdAsync(int disputeId, int residentId);
}
