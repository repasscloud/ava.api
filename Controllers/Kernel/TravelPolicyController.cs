namespace Ava.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelPolicyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TravelPolicyController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/TravelPolicy
    [HttpPost]
    public async Task<IActionResult> CreateTravelPolicy([FromBody] CreateTravelPolicyDTO dto)
    {
        // Ensure the associated AvaClient exists.
        var client = await _context.AvaClients.FindAsync(dto.AvaClientId);
        if (client == null)
        {
            return BadRequest($"AvaClient with Id {dto.AvaClientId} does not exist.");
        }

        string _tpId = Nanoid.Generate(alphabet: Nanoid.Alphabets.HexadecimalUppercase, size: 10);

        var travelPolicy = new TravelPolicy
        {
            Id = _tpId,
            PolicyName = dto.PolicyName,
            AvaClientId = dto.AvaClientId,
            DefaultCurrencyCode = dto.DefaultCurrencyCode,
            MaxFlightPrice = dto.MaxFlightPrice,
            DefaultFlightSeating = dto.DefaultFlightSeating,
            MaxFlightSeating = dto.MaxFlightSeating,
            CabinClassCoverage = dto.CabinClassCoverage,
            NonStopFlight = dto.NonStopFlight,
            EnableSaturdayFlightBookings = dto.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = dto.EnableSundayFlightBookings,
        };

        // add additional optional values
        if (!string.IsNullOrEmpty(dto.IncludedAirlineCodes))
        {
            travelPolicy.IncludedAirlineCodes = dto.IncludedAirlineCodes;
        }

        if (!string.IsNullOrEmpty(dto.ExcludedAirlineCodes))
        {
            travelPolicy.ExcludedAirlineCodes = dto.ExcludedAirlineCodes;
        }

        if (!string.IsNullOrEmpty(dto.FlightBookingTimeAvailableFrom))
        {
            travelPolicy.FlightBookingTimeAvailableFrom = dto.FlightBookingTimeAvailableFrom;
        }

        if (!string.IsNullOrEmpty(dto.FlightBookingTimeAvailableTo))
        {
            travelPolicy.FlightBookingTimeAvailableTo = dto.FlightBookingTimeAvailableTo;
        }

        if (dto.DefaultCalendarDaysInAdvanceForFlightBooking.HasValue)
        {
            travelPolicy.DefaultCalendarDaysInAdvanceForFlightBooking = dto.DefaultCalendarDaysInAdvanceForFlightBooking;
        }

        // Attach Regions if provided.
        if (dto.RegionIds?.Any() == true)
        {
            var regions = await _context.Regions.Where(r => dto.RegionIds.Contains(r.Id)).ToListAsync();
            travelPolicy.Regions = regions;
        }

        // Attach Continents if provided.
        if (dto.ContinentIds?.Any() == true)
        {
            var continents = await _context.Continents.Where(c => dto.ContinentIds.Contains(c.Id)).ToListAsync();
            travelPolicy.Continents = continents;
        }

        // Attach Countries if provided.
        if (dto.CountryIds?.Any() == true)
        {
            var countries = await _context.Countries.Where(c => dto.CountryIds.Contains(c.Id)).ToListAsync();
            travelPolicy.Countries = countries;
        }

        // Add Disabled Countries using the join entity.
        if (dto.DisabledCountryIds?.Any() == true)
        {
            foreach (var countryId in dto.DisabledCountryIds)
            {
                travelPolicy.DisabledCountries.Add(new TravelPolicyDisabledCountry
                {
                    CountryId = countryId,
                    TravelPolicyId = _tpId
                });
            }
        }

        _context.TravelPolicies.Add(travelPolicy);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTravelPolicy), new { id = travelPolicy.Id }, travelPolicy);
    }

    // GET: api/TravelPolicy/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTravelPolicy(string id)
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

    // GET: api/TravelPolicy
    [HttpGet]
    public async Task<IActionResult> GetTravelPolicies()
    {
        var policies = await _context.TravelPolicies
            .Include(tp => tp.Regions)
            .Include(tp => tp.Continents)
            .Include(tp => tp.Countries)
            .Include(tp => tp.DisabledCountries)
            .ToListAsync();
        return Ok(policies);
    }

    // PUT: api/TravelPolicy/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTravelPolicy(string id, [FromBody] UpdateTravelPolicyDto dto)
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

        travelPolicy.PolicyName = dto.PolicyName;

        // Update Regions: Clear then re-add if provided.
        if (dto.RegionIds != null)
        {
            travelPolicy.Regions.Clear();
            var regions = await _context.Regions.Where(r => dto.RegionIds.Contains(r.Id)).ToListAsync();
            travelPolicy.Regions = regions;
        }

        // Update Continents.
        if (dto.ContinentIds != null)
        {
            travelPolicy.Continents.Clear();
            var continents = await _context.Continents.Where(c => dto.ContinentIds.Contains(c.Id)).ToListAsync();
            travelPolicy.Continents = continents;
        }

        // Update Countries.
        if (dto.CountryIds != null)
        {
            travelPolicy.Countries.Clear();
            var countries = await _context.Countries.Where(c => dto.CountryIds.Contains(c.Id)).ToListAsync();
            travelPolicy.Countries = countries;
        }

        // Update Disabled Countries.
        if (dto.DisabledCountryIds != null)
        {
            travelPolicy.DisabledCountries.Clear();
            foreach (var countryId in dto.DisabledCountryIds)
            {
                travelPolicy.DisabledCountries.Add(new TravelPolicyDisabledCountry
                {
                    CountryId = countryId,
                    TravelPolicyId = travelPolicy.Id
                });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/TravelPolicy/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTravelPolicy(string id)
    {
        var travelPolicy = await _context.TravelPolicies.FindAsync(id);
        if (travelPolicy == null)
        {
            return NotFound();
        }

        _context.TravelPolicies.Remove(travelPolicy);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

// DTO for updating a travel policy.
public class UpdateTravelPolicyDto
{
    public required string PolicyName { get; set; }
    public List<int>? RegionIds { get; set; }
    public List<int>? ContinentIds { get; set; }
    public List<int>? CountryIds { get; set; }
    public List<int>? DisabledCountryIds { get; set; }
}
