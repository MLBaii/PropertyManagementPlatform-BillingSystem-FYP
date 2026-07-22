using Microsoft.AspNetCore.Mvc;
using PropertyBill.Api.Services;

namespace PropertyBill.Api.Controllers;

// Not [Authorize] — reachable from the Login screen's "Forgot Password?" link, before a
// resident has a session. This app currently serves a single property (see DbSeeder), so
// there's no per-resident scoping question here yet; revisit if this ever becomes
// multi-property (would need e.g. a propertyId/slug in the route).
[ApiController]
[Route("api/property")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertyController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet("contact")]
    public async Task<IActionResult> GetContact()
    {
        var contact = await _propertyService.GetContactInfoAsync();
        return contact is null ? NotFound() : Ok(contact);
    }
}
