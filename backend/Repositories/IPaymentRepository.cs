using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IPaymentRepository
{
    Task<List<Payment>> GetConfirmedByUnitIdAsync(int unitId);
}
