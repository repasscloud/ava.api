namespace Ava.API.Controllers;

[ApiController]
[Route("api/v1/policies")]
public class PoliciesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PoliciesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/avaClient
    [HttpGet("travel/{id}")]
    public async Task<IActionResult> GetTravelPolicyById(string id)
    {
        var travelPolicy = await _context.TravelPolicies
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
            .FirstOrDefaultAsync(tp => tp.Id == id);

        if (travelPolicy == null)
        {
            return NotFound();
        }

        return Ok(travelPolicy);
    }

    [HttpGet("travel/inter-result/{travelPolicyId}")]
    public async Task<IActionResult> GetTravelPolicyInterResultById(string travelPolicyId)
    {
        var tp = await _context.TravelPolicies
            .Where(c => c.Id == travelPolicyId)
            .Select(tp => new TravelPolicyBookingContextDTO
            {
                Id = tp.Id,
                PolicyName = tp.PolicyName,
                AvaClientId = tp.AvaClientId,
                Currency = tp.DefaultCurrencyCode,
                MaxFlightPrice = tp.MaxFlightPrice,
                DefaultFlightSeating = tp.DefaultFlightSeating,
                MaxFlightSeating = tp.MaxFlightSeating,
                CabinClassCoverage = tp.CabinClassCoverage,
                FlightBookingTimeAvailableFrom = tp.FlightBookingTimeAvailableFrom,
                FlightBookingTimeAvailableTo = tp.FlightBookingTimeAvailableTo,
                IncludedAirlineCodes = tp.IncludedAirlineCodes,
                ExcludedAirlineCodes = tp.ExcludedAirlineCodes,
            })
            .FirstOrDefaultAsync();

        if (tp == null)
        {
            return NotFound();
        }

        return Ok(tp);
    }
}
