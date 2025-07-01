namespace Ava.API.Controllers;

[ApiController]
[Route("api/v1/policies")]
public class PoliciesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILoggerService _loggerService;

    public PoliciesController(ApplicationDbContext context, ILoggerService loggerService)
    {
        _context = context;
        _loggerService = loggerService; ;
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
                AvaClientName = string.Empty,
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

        var clientName = await _context.AvaClients
            .Where(a => a.Id == tp.AvaClientId)
            .Select(a => a.CompanyName)
            .FirstOrDefaultAsync();

        if (clientName == null)
        {
            await _loggerService.LogErrorAsync(
                $"Value 'clientName' is null error for PoliciesController.GetTravelPolicyInterResultById using travelPolicyId='{travelPolicyId}'."
            );
            return NotFound();
        }

        tp.AvaClientName = clientName;

        return Ok(tp);
    }
}
