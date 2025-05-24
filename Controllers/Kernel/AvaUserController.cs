namespace Ava.API.Controllers.Kernel;

[ApiController]
[Route("api/[controller]")]
public class AvaUserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AvaUserController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/avaUser
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvaUser>>> GetUsers()
    {
        return await _context.AvaUsers.ToListAsync();
    }

    // GET: api/avaUser/1
    [HttpGet("{id}")]
    public async Task<ActionResult<AvaUser>> GetUser(int id)
    {
        var avaUser = await _context.AvaUsers.FindAsync(id);
        if (avaUser == null) return NotFound();
        return avaUser;
    }

    // POST: api/avaUser
    [HttpPost]
    public async Task<ActionResult<AvaUser>> CreateUser(AvaUser avaUser)
    {
        // if .ClientId is provided, look up default policy/policyId and attach to avaUser object
        if (avaUser.ClientId is not null)
        {
            var matchedClientRecord = await _context.AvaClients.FirstOrDefaultAsync(c => c.ClientId == avaUser.ClientId);
            if (matchedClientRecord is not null)
            {
                avaUser.TravelPolicy = matchedClientRecord.DefaultTravelPolicy;
                avaUser.TravelPolicyId = matchedClientRecord.DefaultTravelPolicyId;

                // with the TravelPolicy, we can now set other values
                TravelPolicy? policy = await _context.TravelPolicies.FindAsync(avaUser.TravelPolicyId);
                if (policy is not null)
                {
                    // it should NOT be null at this point, but whatever
                    avaUser.DefaultCurrencyCode = policy.DefaultCurrencyCode ?? "AUD"; // either this is set, or it's default to AUD
                    avaUser.DefaultFlightSeating = policy.DefaultFlightSeating;
                    avaUser.MaxFlightSeating = policy.MaxFlightSeating;
                    avaUser.MaxFlightPrice = policy.MaxFlightPrice;
                }
            }
        }

        // if the .ClientId is not provided, or unknown, we can use the email address
        // to find the default policy/policyId and set the .ClientId value too
        var e = avaUser.Email.ToUpperInvariant();
        var domain = e.Substring(e.IndexOf("@") + 1); // get substring after "@"

        var domainObj = await _context.AvaClientSupportedDomains
                                            .Where(x => x.SupportedEmailDomain == domain)
                                            .FirstOrDefaultAsync();

        if (domainObj is not null)
        {
            var clientIdKey = domainObj.AvaClientId;
            var matchedClientRecord = await _context.AvaClients.FirstOrDefaultAsync(c => c.Id == clientIdKey);
            if (matchedClientRecord is not null)
            {
                avaUser.TravelPolicy = matchedClientRecord.DefaultTravelPolicy;
                avaUser.TravelPolicyId = matchedClientRecord.DefaultTravelPolicyId;
                avaUser.ClientId = matchedClientRecord.ClientId;
                avaUser.DefaultCurrencyCode = matchedClientRecord.DefaultTravelPolicy?.DefaultCurrencyCode ?? "AUD";
                avaUser.DefaultFlightSeating = matchedClientRecord.DefaultTravelPolicy?.DefaultFlightSeating ?? "ECONOMY";
                avaUser.MaxFlightSeating = matchedClientRecord.DefaultTravelPolicy?.MaxFlightSeating ?? "ECONOMY";
                avaUser.MaxFlightPrice = matchedClientRecord.DefaultTravelPolicy?.MaxFlightPrice ?? 0;

            }
        }

        // convert name literals to upper case only characters
        avaUser.FirstName = avaUser.FirstName.ToUpperInvariant();
        avaUser.LastName = avaUser.LastName.ToUpperInvariant();
        if (!string.IsNullOrEmpty(avaUser.MiddleName))
        {
            avaUser.MiddleName = avaUser.MiddleName.ToUpperInvariant();
        }

        // save the object to database
        _context.AvaUsers.Add(avaUser);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = avaUser.Id }, avaUser);
    }

    // PUT: api/avaUser/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, AvaUser avaUser)
    {
        if (id != avaUser.Id) return BadRequest();

        _context.Entry(avaUser).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/avaUser/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var avaUser = await _context.AvaUsers.FindAsync(id);
        if (avaUser == null) return NotFound();

        _context.AvaUsers.Remove(avaUser);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // uses the AspNetUsersId value
    // GET: api/avaUser/userpref/{aspNetUsersId}
    [HttpGet("userpref/{aspNetUsersId}")]
    public async Task<ActionResult<AvaUserSysPreference>> GetUser(string aspNetUsersId)
    {
        var userPref = await _context.AvaUserSysPreferences
            .Where(x => x.AspNetUsersId == aspNetUsersId)
            .FirstOrDefaultAsync();

        if (userPref == null) return NotFound();
        return userPref;
    }
}
