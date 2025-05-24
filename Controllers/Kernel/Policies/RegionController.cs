namespace Ava.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public RegionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Region
    [HttpGet]
    public async Task<IActionResult> GetRegions()
    {
        var regions = await _context.Regions
            .Include(r => r.Continents)
            .ToListAsync();
        return Ok(regions);
    }

    // GET: api/Region/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRegion(int id)
    {
        var region = await _context.Regions
            .Include(r => r.Continents)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (region == null)
            return NotFound();
        return Ok(region);
    }

    // POST: api/Region
    [HttpPost]
    public async Task<IActionResult> CreateRegion([FromBody] CreateRegionDto dto)
    {
        var region = new Region
        {
            Name = dto.Name
        };

        _context.Regions.Add(region);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRegion), new { id = region.Id }, region);
    }

    // PUT: api/Region/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRegion(int id, [FromBody] UpdateRegionDto dto)
    {
        var region = await _context.Regions.FindAsync(id);
        if (region == null)
            return NotFound();
        
        region.Name = dto.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Region/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRegion(int id)
    {
        var region = await _context.Regions
            .Include(r => r.Continents)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (region == null)
            return NotFound();

        if (region.Continents.Any())
        {
            return BadRequest("Cannot delete region with associated continents.");
        }

        _context.Regions.Remove(region);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateRegionDto
{
    public required string Name { get; set; }
}

public class UpdateRegionDto
{
    public required string Name { get; set; }
}
