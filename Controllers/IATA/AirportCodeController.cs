namespace Ava.API.Controllers.IATA;

[Route("api/[controller]")]
[ApiController]
public class AirportCodeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AirportCodeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/IATAAirportCodess
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IATAAirportCodes>>> GetIATAAirportCodess()
    {
        return await _context.IATAAirportCodes.ToListAsync();
    }

    // GET: api/IATAAirportCodess/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<IATAAirportCodes>> GetIATAAirportCodes(int id)
    {
        var data = await _context.IATAAirportCodes.FindAsync(id);

        if (data == null)
        {
            return NotFound();
        }

        return data;
    }

    // POST: api/IATAAirportCodess
    [HttpPost]
    public async Task<ActionResult<IATAAirportCodes>> PostIATAAirportCodes(IATAAirportCodes data)
    {
        _context.IATAAirportCodes.Add(data);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetIATAAirportCodes), new { id = data.Id }, data);
    }

    // PUT: api/IATAAirportCodess/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutIATAAirportCodes(int id, IATAAirportCodes data)
    {
        if (id != data.Id)
        {
            return BadRequest();
        }

        // Ensure unique SupportedEmaildata (if changed)
        if (await _context.IATAAirportCodes.AnyAsync(d => d.Identity == data.Identity && d.Id != id))
        {
            return Conflict(new { message = "The identity data must be unique" });
        }

        _context.Entry(data).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!IATAAirportCodesExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/IATAAirportCodess/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIATAAirportCodes(int id)
    {
        var data = await _context.IATAAirportCodes.FindAsync(id);
        if (data == null)
        {
            return NotFound();
        }

        _context.IATAAirportCodes.Remove(data);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool IATAAirportCodesExists(int id)
    {
        return _context.IATAAirportCodes.Any(e => e.Id == id);
    }
}
