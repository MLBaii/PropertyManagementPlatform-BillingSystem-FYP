using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IResidentRepository
{
    Task<Resident?> GetByEmailAsync(string email);
    Task<Resident?> GetByIdAsync(int residentId);
    Task<bool> ExistsWithEmailAsync(string email, int excludingResidentId);
    Task SaveChangesAsync();
}
