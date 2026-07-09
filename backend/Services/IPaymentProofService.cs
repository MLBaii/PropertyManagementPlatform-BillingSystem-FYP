using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IPaymentProofService
{
    Task<PaymentProofSubmitResult> SubmitAsync(int residentId, int unitId, IFormFile? file, List<int> billIds);
    Task<List<PaymentProofDto>> GetHistoryAsync(int residentId);
}
