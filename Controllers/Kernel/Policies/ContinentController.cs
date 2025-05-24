namespace Ava.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContinentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ContinentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Continent
    [HttpGet]
    public async Task<IActionResult> GetContinents()
    {
        var continents = await _context.Continents
            .Include(c => c.Region)
            .Include(c => c.Countries)
            .ToListAsync();
        return Ok(continents);
    }

    // GET: api/Continent/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetContinent(int id)
    {
        var continent = await _context.Continents
            .Include(c => c.Region)
            .Include(c => c.Countries)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (continent == null)
            return NotFound();
        return Ok(continent);
    }

    // POST: api/Continent
    [HttpPost]
    public async Task<IActionResult> CreateContinent([FromBody] CreateContinentDto dto)
    {
        // If RegionId provided, validate its existence.
        if (dto.RegionId.HasValue)
        {
            var region = await _context.Regions.FindAsync(dto.RegionId.Value);
            if (region == null)
                return BadRequest($"Region with id {dto.RegionId.Value} does not exist.");
        }

        var continent = new Continent
        {
            Name = dto.Name,
            IsoCode = dto.ContinentCode,
            RegionId = dto.RegionId
        };

        _context.Continents.Add(continent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContinent), new { id = continent.Id }, continent);
    }

    // // POST: api/Continent/{region}
    // [HttpPost("{region}")]
    // public async Task<IActionResult> CreateContinentUnderRegion(string region, [FromBody] CreateContinentDto dto)
    // {
    //     // Attempt to find an existing region (case-insensitive search)
    //     var regionEntity = await _context.Regions
    //         .FirstOrDefaultAsync(r => r.Name.Equals(region, StringComparison.OrdinalIgnoreCase));

    //     // If the region does not exist, create it and then look it up again to retrieve the generated Id
    //     if (regionEntity == null)
    //     {
    //         var newRegion = new Region
    //         {
    //             Name = region.ToUpper()  // use the route parameter for the region name
    //         };

    //         _context.Regions.Add(newRegion);
    //         await _context.SaveChangesAsync();

    //         // Lookup the region again to get the Id (ensuring it's updated)
    //         regionEntity = await _context.Regions
    //             .FirstOrDefaultAsync(r => r.Name.Equals(region.ToUpper(), StringComparison.OrdinalIgnoreCase));

    //         if (regionEntity == null)
    //         {
    //             return BadRequest("Failed to create and retrieve the region.");
    //         }
    //     }

    //     // Create the continent associated with the region
    //     var continent = new Continent
    //     {
    //         Name = dto.Name,
    //         IsoCode = dto.ContinentCode,
    //         RegionId = regionEntity.Id,
    //     };

    //     _context.Continents.Add(continent);
    //     await _context.SaveChangesAsync();

    //     return CreatedAtAction(nameof(GetContinent), new { id = continent.Id }, continent);
    // }

    // PUT: api/Continent/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContinent(int id, [FromBody] UpdateContinentDto dto)
    {
        var continent = await _context.Continents.FindAsync(id);
        if (continent == null)
            return NotFound();

        continent.Name = dto.Name;
        continent.IsoCode = dto.ContinentCode;
        continent.RegionId = dto.RegionId;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Continent/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContinent(int id)
    {
        var continent = await _context.Continents
            .Include(c => c.Countries)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (continent == null)
            return NotFound();

        if (continent.Countries.Any())
        {
            return BadRequest("Cannot delete continent with associated countries.");
        }

        _context.Continents.Remove(continent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateContinentDto
{
    public required string Name { get; set; }
    public required string ContinentCode { get; set; }
    public int? RegionId { get; set; }
}

public class UpdateContinentDto
{
    public required string Name { get; set; }
    public required string ContinentCode { get; set; }
    public int? RegionId { get; set; }
}
