using PropertyBill.Api.Dtos;

namespace PropertyBill.Api.Services;

public interface IPropertyService
{
    Task<PropertyContactDto?> GetContactInfoAsync();
}
