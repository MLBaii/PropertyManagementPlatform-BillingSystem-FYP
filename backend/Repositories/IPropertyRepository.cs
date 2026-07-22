using PropertyBill.Api.Models;

namespace PropertyBill.Api.Repositories;

public interface IPropertyRepository
{
    Task<Property?> GetFirstAsync();
}
