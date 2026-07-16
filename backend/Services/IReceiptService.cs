using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IReceiptService
{
    Task<List<ReceiptDto>> GetReceiptsAsync(int unitId);
    Task<ReceiptDetailDto?> GetReceiptDetailAsync(int paymentId, int unitId);
}
