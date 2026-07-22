using PropertyBill.Api.Dtos;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyContactDto?> GetContactInfoAsync()
    {
        var property = await _propertyRepository.GetFirstAsync();
        if (property is null)
        {
            return null;
        }

        return new PropertyContactDto
        {
            PropertyName = property.Name,
            ContactEmail = property.ContactEmail,
            ContactPhone = property.ContactPhone,
        };
    }
}
