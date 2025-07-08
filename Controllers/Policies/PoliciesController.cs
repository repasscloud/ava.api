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
            await _loggerService.LogErrorAsync(
                $"TravelPolicy not found with TravelPolicyId={id}."
            );
            return NotFound();
        }

        return Ok(travelPolicy);
    }

    [HttpGet("travel/inter-result/{travelPolicyId}")]
    public async Task<IActionResult> GetTravelPolicyInterResultById(string travelPolicyId)
    {
        var tp = await _context.TravelPolicies
                    .Include(tp => tp.Regions)
                    .Include(tp => tp.Continents)
                    .Include(tp => tp.Countries)
                    .Include(tp => tp.DisabledCountries)
                    .FirstOrDefaultAsync(tp => tp.Id == travelPolicyId);

        if (tp == null)
        {
            await _loggerService.LogErrorAsync(
                $"TravelPolicy not found using PoliciesController.GetTravelPolicyInterResultById using travelPolicyId={travelPolicyId}."
            );
            return NotFound();
        }

        var clientName = await _context.AvaClients
            .Where(a => a.Id == tp.AvaClientId)
            .Select(a => a.CompanyName)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(clientName))
        {
            await _loggerService.LogErrorAsync(
                $"Value 'clientName' is null error for PoliciesController.GetTravelPolicyInterResultById using travelPolicyId='{travelPolicyId}'."
            );
            return NotFound();
        }

        var regionNames = tp.Regions.Select(r => r.Name).ToList();
        var continentNames = tp.Continents.Select(c => c.Name).ToList();
        var countryNames = tp.Countries.Select(c => c.Name).ToList();
        var disabledCountryNames = tp.DisabledCountries.Select(c => c.Country?.Name).ToList();

        var travelPolicy = new TravelPolicyBookingContextDTO
        {
            Id = tp.Id,
            PolicyName = tp.PolicyName,
            AvaClientId = tp.AvaClientId,
            AvaClientName = clientName,
            Currency = tp.DefaultCurrencyCode,
            MaxFlightPrice = tp.MaxFlightPrice,
            DefaultFlightSeating = tp.DefaultFlightSeating,
            MaxFlightSeating = tp.MaxFlightSeating,
            CabinClassCoverage = tp.CabinClassCoverage,
            NonStopFlight = tp.NonStopFlight,
            FlightBookingTimeAvailableFrom = tp.FlightBookingTimeAvailableFrom,
            FlightBookingTimeAvailableTo = tp.FlightBookingTimeAvailableTo,
            EnableSaturdayFlightBookings = tp.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = tp.EnableSundayFlightBookings,
            DefaultCalendarDaysInAdvanceForFlightBooking = tp.DefaultCalendarDaysInAdvanceForFlightBooking,
            IncludedAirlineCodes = tp.IncludedAirlineCodes,
            ExcludedAirlineCodes = tp.ExcludedAirlineCodes,
            Regions = regionNames,
            Continents = continentNames,
            Countries = countryNames,
            MaxResults = tp.MaxResults,
        };

        if (disabledCountryNames?.Count > 0)
        {
            travelPolicy.DisabledCountries = disabledCountryNames!;
        }

        return Ok(travelPolicy);
    }
}
