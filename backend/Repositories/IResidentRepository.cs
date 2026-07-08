using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IResidentRepository
{
    Task<Resident?> GetByEmailAsync(string email);
}
