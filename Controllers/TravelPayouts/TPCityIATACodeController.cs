namespace Ava.API.Controllers.Kernel;

[ApiController]
[Route("api/tp/iata/city-codes")]
public class TPCityIATACodeController : ControllerBase
{
    private readonly ITPCityIATACodeService _cityService;
    private readonly ApplicationDbContext _context;

    public TPCityIATACodeController(ITPCityIATACodeService cityService, ApplicationDbContext context)
    {
        _cityService = cityService;
        _context = context;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncCities()
    {
        await _cityService.SyncCitiesDataAsync();
        return Ok(new { message = "City data synchronized successfully." });
    }

    // get city by IATA code (3-letter airport/city code)
    [HttpGet("{code}")]
    public async Task<IActionResult> GetCityByIATACode(string code)
    {
        // Validate: Ensure code is exactly 3 alphanumeric characters
        if (string.IsNullOrWhiteSpace(code) || !Regex.IsMatch(code, "^[a-zA-Z0-9]{3}$"))
        {
            return BadRequest(new { message = "Invalid Code. Must be exactly 3 alphanumeric characters." });
        }

        // Convert to uppercase
        code = code.ToUpper();

        // Fetch city from database
        var city = await _context.TPCityIATACodes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code);

        if (city == null)
        {
            return NotFound(new { message = $"No city found for IATA code: {code}" });
        }

        return Ok(city);
    }
}
