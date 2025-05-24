namespace Ava.API.Controllers.Client;

[Route("api/[controller]")]
[ApiController]
public class ClientSupportedDomainsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientSupportedDomainsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ClientSupportedDomains
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvaClientSupportedDomain>>> GetClientSupportedDomains()
    {
        return await _context.AvaClientSupportedDomains.ToListAsync();
    }

    // GET: api/ClientSupportedDomains/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<AvaClientSupportedDomain>> GetClientSupportedDomain(string id)
    {
        var domain = await _context.AvaClientSupportedDomains.FindAsync(id);

        if (domain == null)
        {
            return NotFound();
        }

        return domain;
    }

    // POST: api/ClientSupportedDomains/{clientId}/{emailDomain}
    [HttpPost("{clientId}/{emailDomain}")]
    public async Task<ActionResult<AvaClientSupportedDomain>> AddDomainByClientId(string clientId, string emailDomain)
    {
        // Ensure unique SupportedEmailDomain
        if (await _context.AvaClientSupportedDomains
            .AnyAsync(d => d.SupportedEmailDomain == emailDomain.ToUpperInvariant()))
        {
            return Conflict(new { message = "The email domain must be unique." });
        }

        // find the client by clientId first
        var clientFound = _context.AvaClients.Where(c => c.ClientId == clientId).FirstOrDefault();
        if (clientFound is null)
        {
            return Problem(
                detail: "ClientId not found.",
                statusCode: 418,
                title: "Custom Error Title",
                type: "https://example.com/errors/custom-error"
            );
        }

        // now all validation passed, construct the object to add to the DB
        AvaClientSupportedDomain domain = new AvaClientSupportedDomain
        {
            Id = 0,
            SupportedEmailDomain = emailDomain.ToUpperInvariant(),
            AvaClientId = clientFound.Id,
            AvaClient = clientFound,
            ClientCode = clientFound.ClientId
        };

        _context.AvaClientSupportedDomains.Add(domain);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClientSupportedDomain), new { id = domain.Id }, domain);
    }

    // POST: api/ClientSupportedDomains/email
    [HttpPost("email")]
    public async Task<ActionResult<AvaClientSupportedDomain>> GetClientSupportedDomainByEmail([FromBody] EmailRequestDTO request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        {
            return BadRequest("Invalid email address format.");
        }

        var e = request.Email.ToUpperInvariant();
        var domain = e.Substring(e.IndexOf("@") + 1); // Get substring after "@"

        var result = await _context.AvaClientSupportedDomains
                                    .Where(x => x.SupportedEmailDomain == domain)
                                    .FirstOrDefaultAsync();

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result); // Return the found SupportedEmailDomain
    }


    // POST: api/ClientSupportedDomains
    [HttpPost]
    public async Task<ActionResult<AvaClientSupportedDomain>> PostClientSupportedDomain(AvaClientSupportedDomain domain)
    {
        // Ensure unique SupportedEmailDomain
        if (await _context.AvaClientSupportedDomains.AnyAsync(d => d.SupportedEmailDomain == domain.SupportedEmailDomain.ToUpperInvariant()))
        {
            return Conflict(new { message = "The email domain must be unique." });
        }

        domain.Id = 0;
        domain.SupportedEmailDomain = domain.SupportedEmailDomain.ToUpperInvariant();  // change to upper case
        _context.AvaClientSupportedDomains.Add(domain);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClientSupportedDomain), new { id = domain.Id }, domain);
    }

    // PUT: api/ClientSupportedDomains/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutClientSupportedDomain(int id, AvaClientSupportedDomain domain)
    {
        if (id != domain.Id)
        {
            return BadRequest();
        }

        // Ensure unique SupportedEmailDomain (if changed)
        if (await _context.AvaClientSupportedDomains.AnyAsync(d => d.SupportedEmailDomain == domain.SupportedEmailDomain && d.Id != id))
        {
            return Conflict(new { message = "The email domain must be unique." });
        }

        _context.Entry(domain).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClientSupportedDomainExists(id))
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

    // DELETE: api/ClientSupportedDomains/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClientSupportedDomain(int id)
    {
        var domain = await _context.AvaClientSupportedDomains.FindAsync(id);
        if (domain == null)
        {
            return NotFound();
        }

        _context.AvaClientSupportedDomains.Remove(domain);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ClientSupportedDomainExists(int id)
    {
        return _context.AvaClientSupportedDomains.Any(e => e.Id == id);
    }
}
