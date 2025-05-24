namespace Ava.Api.Controllers.Wikipedia;

[ApiController]
[Route("api/v1/wikipedia")]
public class AircraftTypeDesignatorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AircraftTypeDesignatorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/v1/wikipedia/aircrafttypedesignators
    [HttpGet("aircrafttypedesignators")]
    public async Task<ActionResult<IEnumerable<AircraftTypeDesignator>>> GetAll()
    {
        return await _context.AircraftTypeDesignators.AsNoTracking().ToListAsync();
    }

    // GET: api/v1/wikipedia/aircrafttypedesignators/5
    [HttpGet("aircrafttypedesignators/{id}")]
    public async Task<ActionResult<AircraftTypeDesignator>> GetById(int id)
    {
        var item = await _context.AircraftTypeDesignators.FindAsync(id);
        if (item == null)
            return NotFound();

        return item;
    }

    // POST: api/v1/wikipedia/aircrafttypedesignators
    [HttpPost("aircrafttypedesignators")]
    public async Task<ActionResult<AircraftTypeDesignator>> Create(AircraftTypeDesignator designator)
    {
        _context.AircraftTypeDesignators.Add(designator);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = designator.Id }, designator);
    }

    // PUT: api/v1/wikipedia/aircrafttypedesignators/5
    [HttpPut("aircrafttypedesignators/{id}")]
    public async Task<IActionResult> Update(int id, AircraftTypeDesignator designator)
    {
        if (id != designator.Id)
            return BadRequest("ID mismatch");

        _context.Entry(designator).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DesignatorExists(id))
                return NotFound();

            throw;
        }

        return NoContent();
    }

    // DELETE: api/v1/wikipedia/aircrafttypedesignators/5
    [HttpDelete("aircrafttypedesignators/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var designator = await _context.AircraftTypeDesignators.FindAsync(id);
        if (designator == null)
            return NotFound();

        _context.AircraftTypeDesignators.Remove(designator);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DesignatorExists(int id) =>
        _context.AircraftTypeDesignators.Any(e => e.Id == id);
}
