namespace Ava.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public CountryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Country
    [HttpGet]
    public async Task<IActionResult> GetCountries()
    {
        var countries = await _context.Countries
            .Include(c => c.Continent)
            .ToListAsync();
        return Ok(countries);
    }

    // GET: api/Country/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCountry(int id)
    {
        var country = await _context.Countries
            .Include(c => c.Continent)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (country == null)
            return NotFound();
        return Ok(country);
    }

    // GET: api/Country/isoCode/{isoCode}
    [HttpGet("isoCode/{isoCode}")]
    public async Task<IActionResult> GetCountryByIsoCode(string isoCode)
    {
        var country = await _context.Countries
            .Where(c => c.IsoCode == isoCode.ToUpperInvariant())
            .Include(c => c.Continent)
            .FirstOrDefaultAsync();
        if (country == null)
            return NotFound();
        return Ok(country);
    }

    // POST: api/Country
    [HttpPost]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDto dto)
    {
        // If ContinentId provided, validate its existence.
        if (dto.ContinentId.HasValue)
        {
            var continent = await _context.Continents.FindAsync(dto.ContinentId.Value);
            if (continent == null)
                return BadRequest($"Continent with id {dto.ContinentId.Value} does not exist.");
        }

        var country = new Country
        {
            Name = dto.Name,
            IsoCode = dto.CountryCode,
            Flag = dto.CountryFlag,
            ContinentId = dto.ContinentId
        };

        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
    }

    // PUT: api/Country/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDto dto)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();
        
        country.Name = dto.Name;
        country.IsoCode = dto.CountryCode;
        country.Flag = dto.CountryFlag;
        country.ContinentId = dto.ContinentId;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Country/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var country = await _context.Countries.FindAsync(id);
        if (country == null)
            return NotFound();

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateCountryDto
{
    public required string Name { get; set; }
    public required string CountryCode { get; set; }
    public required string CountryFlag { get; set; }
    public int? ContinentId { get; set; }
}

public class UpdateCountryDto
{
    public required string Name { get; set; }
    public required string CountryCode { get; set; }
    public required string CountryFlag { get; set; }
    public int? ContinentId { get; set; }
}
