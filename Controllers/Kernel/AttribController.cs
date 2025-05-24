namespace Ava.API.Controllers.Kernel;

[ApiController]
public class AttribController : ControllerBase
{
    private readonly ApplicationDbContext _context; // or whatever your context is called

    public AttribController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ───────────────────────────────────────────────────────────────
    // SupportedCountry Endpoints
    // ───────────────────────────────────────────────────────────────

    [HttpGet("~/api/v1/attrib/countries")]
    public async Task<ActionResult<IEnumerable<SupportedCountry>>> GetCountries()
        => await _context.SupportedCountries.ToListAsync();

    [HttpGet("~/api/v1/attrib/countries/{id:int}")]
    public async Task<ActionResult<SupportedCountry>> GetCountry(int id)
    {
        var item = await _context.SupportedCountries.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost("~/api/v1/attrib/countries")]
    public async Task<ActionResult<SupportedCountry>> CreateCountry([FromBody] SupportedCountry country)
    {
        _context.SupportedCountries.Add(country);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
    }

    [HttpPut("~/api/v1/attrib/countries/{id:int}")]
    public async Task<IActionResult> UpdateCountry(int id, [FromBody] SupportedCountry country)
    {
        if (id != country.Id) return BadRequest();
        _context.Entry(country).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) when (!_context.SupportedCountries.Any(e => e.Id == id))
        { return NotFound(); }
        return NoContent();
    }

    [HttpDelete("~/api/v1/attrib/countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var entity = await _context.SupportedCountries.FindAsync(id);
        if (entity is null) return NotFound();
        _context.SupportedCountries.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────────────────────────────────────────────
    // SupportedCurrency Endpoints
    // ───────────────────────────────────────────────────────────────

    [HttpGet("~/api/v1/attrib/currencies")]
    public async Task<ActionResult<IEnumerable<SupportedCurrency>>> GetCurrencies()
        => await _context.SupportedCurrencies.ToListAsync();

    [HttpGet("~/api/v1/attrib/currencies/{id:int}")]
    public async Task<ActionResult<SupportedCurrency>> GetCurrency(int id)
    {
        var item = await _context.SupportedCurrencies.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost("~/api/v1/attrib/currencies")]
    public async Task<ActionResult<SupportedCurrency>> CreateCurrency([FromBody] SupportedCurrency currency)
    {
        _context.SupportedCurrencies.Add(currency);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCurrency), new { id = currency.Id }, currency);
    }

    [HttpPut("~/api/v1/attrib/currencies/{id:int}")]
    public async Task<IActionResult> UpdateCurrency(int id, [FromBody] SupportedCurrency currency)
    {
        if (id != currency.Id) return BadRequest();
        _context.Entry(currency).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) when (!_context.SupportedCurrencies.Any(e => e.Id == id))
        { return NotFound(); }
        return NoContent();
    }

    [HttpDelete("~/api/v1/attrib/currencies/{id:int}")]
    public async Task<IActionResult> DeleteCurrency(int id)
    {
        var entity = await _context.SupportedCurrencies.FindAsync(id);
        if (entity is null) return NotFound();
        _context.SupportedCurrencies.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────────────────────────────────────────────
    // SupportedDialCode Endpoints
    // ───────────────────────────────────────────────────────────────

    [HttpGet("~/api/v1/attrib/dialcodes")]
    public async Task<ActionResult<IEnumerable<SupportedDialCode>>> GetDialCodes()
        => await _context.SupportedDialCodes.ToListAsync();

    [HttpGet("~/api/v1/attrib/dialcodes/{id:int}")]
    public async Task<ActionResult<SupportedDialCode>> GetDialCode(int id)
    {
        var item = await _context.SupportedDialCodes.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost("~/api/v1/attrib/dialcodes")]
    public async Task<ActionResult<SupportedDialCode>> CreateDialCode([FromBody] SupportedDialCode dialCode)
    {
        _context.SupportedDialCodes.Add(dialCode);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDialCode), new { id = dialCode.Id }, dialCode);
    }

    [HttpPut("~/api/v1/attrib/dialcodes/{id:int}")]
    public async Task<IActionResult> UpdateDialCode(int id, [FromBody] SupportedDialCode dialCode)
    {
        if (id != dialCode.Id) return BadRequest();
        _context.Entry(dialCode).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) when (!_context.SupportedDialCodes.Any(e => e.Id == id))
        { return NotFound(); }
        return NoContent();
    }

    [HttpDelete("~/api/v1/attrib/dialcodes/{id:int}")]
    public async Task<IActionResult> DeleteDialCode(int id)
    {
        var entity = await _context.SupportedDialCodes.FindAsync(id);
        if (entity is null) return NotFound();
        _context.SupportedDialCodes.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ───────────────────────────────────────────────────────────────
    // SupportedTaxId Endpoints
    // ───────────────────────────────────────────────────────────────

    [HttpGet("~/api/v1/attrib/taxids")]
    public async Task<ActionResult<IEnumerable<SupportedTaxId>>> GetTaxIds()
        => await _context.SupportedTaxIds.ToListAsync();

    [HttpGet("~/api/v1/attrib/taxids/{id:int}")]
    public async Task<ActionResult<SupportedTaxId>> GetTaxId(int id)
    {
        var item = await _context.SupportedTaxIds.FindAsync(id);
        return item is null ? NotFound() : item;
    }

    [HttpPost("~/api/v1/attrib/taxids")]
    public async Task<ActionResult<SupportedTaxId>> CreateTaxId([FromBody] SupportedTaxId taxId)
    {
        _context.SupportedTaxIds.Add(taxId);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTaxId), new { id = taxId.Id }, taxId);
    }

    [HttpPut("~/api/v1/attrib/taxids/{id:int}")]
    public async Task<IActionResult> UpdateTaxId(int id, [FromBody] SupportedTaxId taxId)
    {
        if (id != taxId.Id) return BadRequest();
        _context.Entry(taxId).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) when (!_context.SupportedTaxIds.Any(e => e.Id == id))
        { return NotFound(); }
        return NoContent();
    }

    [HttpDelete("~/api/v1/attrib/taxids/{id:int}")]
    public async Task<IActionResult> DeleteTaxId(int id)
    {
        var entity = await _context.SupportedTaxIds.FindAsync(id);
        if (entity is null) return NotFound();
        _context.SupportedTaxIds.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
