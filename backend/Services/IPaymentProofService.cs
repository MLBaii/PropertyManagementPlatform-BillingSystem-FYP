using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IPaymentProofService
{
    Task<PaymentProofSubmitResult> SubmitAsync(int residentId, int unitId, List<IFormFile>? files, List<int> billIds);
    Task<List<PaymentProofDto>> GetHistoryAsync(int residentId);
}
