namespace Ava.Api.Controllers;

[ApiController]
[Route("api/v1/travelsearch")]
public class TravelSearchRecordsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TravelSearchRecordsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/TravelSearchRecords
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TravelSearchRecord>>> GetAll()
    {
        return await _context.TravelSearchRecords.ToListAsync();
    }

    // GET: api/TravelSearchRecords/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<TravelSearchRecord>> Get(long id)
    {
        var record = await _context.TravelSearchRecords.FindAsync(id);

        if (record == null)
            return NotFound();

        return record;
    }

    // POST: api/TravelSearchRecords
    [HttpPost]
    public async Task<ActionResult<TravelSearchRecord>> Create(TravelSearchRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.ExpiresAt = record.CreatedAt.AddDays(30);

        _context.TravelSearchRecords.Add(record);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = record.Id }, record);
    }

    // PUT: api/TravelSearchRecords/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, TravelSearchRecord updated)
    {
        if (id != updated.Id)
            return BadRequest("ID mismatch");

        var record = await _context.TravelSearchRecords.FindAsync(id);
        if (record == null)
            return NotFound();

        // Update properties
        record.SearchId = updated.SearchId;
        record.TravelType = updated.TravelType;
        record.FlightSubComponent = updated.FlightSubComponent;
        record.HotelSubComponent = updated.HotelSubComponent;
        record.CarSubComponent = updated.CarSubComponent;
        record.RailSubComponent = updated.RailSubComponent;
        record.TransferSubComponent = updated.TransferSubComponent;
        record.ActivitySubComponent = updated.ActivitySubComponent;
        record.Payload = updated.Payload;
        record.ExpiresAt = updated.ExpiresAt;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/TravelSearchRecords/5
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var record = await _context.TravelSearchRecords.FindAsync(id);
        if (record == null)
            return NotFound();

        _context.TravelSearchRecords.Remove(record);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
