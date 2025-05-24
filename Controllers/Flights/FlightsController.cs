namespace Ava.API.Controllers.Flights;

[Route("api/v1/flights")]
[ApiController]
public class FlightsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAmadeusAuthService _authService;  // for the amadeus search itself
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly ILoggerService _loggerService;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public FlightsController(
        ApplicationDbContext context,
        IAmadeusAuthService authService,
        IAmadeusFlightSearchService flightSearchService,
        ILoggerService loggerService,
        JsonSerializerOptions jsonOptions)
    {
        _context = context;
        _authService = authService;
        _flightSearchService = flightSearchService;
        _loggerService = loggerService;
        _jsonOptions = jsonOptions;
    }
    
    // POST: api/v1/flights/search
    [HttpPost("search")]
    public async Task<IActionResult> SearchFlights(FlightOfferSearchRequestDTO criteria)
    {
        // add .CreatedAt value (this must be controlled by API, it will be ignored by everything else)
        criteria.CreatedAt = DateTime.UtcNow;

        // check that a record doesn't match the criteria.Id first, else throw an error
        var existing = await _context.FlightOfferSearchRequestDTOs
            .FirstOrDefaultAsync(x => x.Id == criteria.Id);

        if (existing is not null)
        {
            await _loggerService.LogCriticalAsync($"Table 'FLightOfferSearchRequestDTOs' has matching value for {criteria.Id}");
            return BadRequest($"A record with Id = {criteria.Id} already exists.");
        }

        // save it to the database
        await _loggerService.LogInfoAsync($"Received flight search record with ID '{criteria.Id}' created at '{criteria.CreatedAt}'");

        // save the search to history (to be used later?)
        _context.FlightOfferSearchRequestDTOs.Add(criteria);
        await _context.SaveChangesAsync();
        await _loggerService.LogDebugAsync($"Executing flight search record with ID: {criteria.Id}");

        // create the payload with defaults, and empty payload
        TravelSearchRecord travelSearchRecord = new TravelSearchRecord
        {
            Id = 0,
            SearchId = criteria.Id,
            TravelType = TravelComponentType.Flight,
            FlightSubComponent = FlightSubComponentType.None,
            HotelSubComponent = HotelSubComponentType.None,
            CarSubComponent = CarSubComponentType.None,
            RailSubComponent = RailSubComponentType.None,
            TransferSubComponent = TransferSubComponentType.None,
            ActivitySubComponent = ActivitySubComponentType.None,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Payload = string.Empty,
        };

        if (criteria.IsOneWay)
        {
            // if true, update with .Flight_OneWay enum value
            travelSearchRecord.FlightSubComponent = FlightSubComponentType.Flight_OneWay;

            // all logging happns in the _flightSearchService for the .GetFlightOffersAsync() activity
            var response = await _flightSearchService.GetFlightOffersAsync(criteria);

            if (response is not null)
            {
                await _loggerService.LogDebugAsync($"Sending API response for flight search record with ID: {criteria.Id}");
                
                // update travelSearchRecord and save to DB
                travelSearchRecord.Payload = JsonSerializer.Serialize(response);
                _context.TravelSearchRecords.Add(travelSearchRecord);
                await _context.SaveChangesAsync();

                // create wrapper for API results to be returned
                var wrapper = new
                {
                    TravelSearchRecord = criteria.Id
                };
                
                return Ok(wrapper); // returns HTTP 200 with JSON
            }
            else
            {
                return NotFound("The response from the Amadeus API was empty or invalid.");
            }
        }
        else
        {
            // if false, update with .Flight_Return enum value
            travelSearchRecord.FlightSubComponent = FlightSubComponentType.Flight_Return;

            // create a split in the criteria, making criteria01 (one way)
            var criteria01 = new FlightOfferSearchRequestDTO
            {
                Id = criteria.Id,
                CreatedAt = criteria.CreatedAt,
                ClientId = criteria.ClientId,
                CustomerId = criteria.CustomerId,
                TravelPolicyId = criteria.TravelPolicyId,
                OriginLocationCode = criteria.OriginLocationCode,
                DestinationLocationCode = criteria.DestinationLocationCode,
                IsOneWay = true,
                DepartureDate = criteria.DepartureDate,
                DepartureDateReturn = null,
                Adults = criteria.Adults,
                CabinClass = criteria.CabinClass
            };

            // create a split in the criteria, making criteria02 (return)
            var criteria02 = new FlightOfferSearchRequestDTO
            {
                Id = criteria.Id,
                CreatedAt = criteria.CreatedAt,
                ClientId = criteria.ClientId,
                CustomerId = criteria.CustomerId,
                TravelPolicyId = criteria.TravelPolicyId,
                OriginLocationCode = criteria.DestinationLocationCode,
                DestinationLocationCode = criteria.OriginLocationCode,
                IsOneWay = true,
                DepartureDate = criteria.DepartureDateReturn!,
                DepartureDateReturn = null,
                Adults = criteria.Adults,
                CabinClass = criteria.CabinClass
            };

            // all logging happns in the _flightSearchService for the .GetFlightOffersAsync() activity
            var response01 = await _flightSearchService.GetFlightOffersAsync(criteria01);
            var response02 = await _flightSearchService.GetFlightOffersAsync(criteria02);

            if (response01 is not null && response02 is not null)
            {
                await _loggerService.LogDebugAsync($"Sending API response for flight search record 01 of 02 with ID: {criteria.Id}");
                await _loggerService.LogDebugAsync($"Sending API response for flight search record 02 of 02 with ID: {criteria.Id}");

                List<AmadeusFlightOfferSearchResult> results = new List<AmadeusFlightOfferSearchResult>
                {
                    response01,
                    response02
                };

                // update travelSearchRecord and save to DB
                travelSearchRecord.Payload = JsonSerializer.Serialize(results);
                _context.TravelSearchRecords.Add(travelSearchRecord);
                await _context.SaveChangesAsync();

                // create wrapper for API results to be returned
                var wrapper = new
                {
                    TravelSearchRecord = criteria.Id
                };

                return Ok(wrapper); // returns HTTP 200 with JSON
            }
            else
            {
                return NotFound("The response from the Amadeus API was empty or invalid.");
            }
        }
    }
}
