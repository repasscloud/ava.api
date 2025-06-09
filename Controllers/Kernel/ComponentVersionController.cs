namespace Ava.API.Controllers.Kernel;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class ComponentVersionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ComponentVersionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ComponentVersion
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VersionInfo>>> GetAllVersionsAsync()
    {
        return await _context.VersionInfos.ToListAsync();
    }

    // GET: api/ComponentVersion/5
    [HttpGet("{id}")]
    public async Task<ActionResult<VersionInfo>> GetVersionAsync(int id)
    {
        var version = await _context.VersionInfos.FindAsync(id);
        if (version == null) return NotFound();
        return version;
    }

    // POST: api/ComponentVersion
    [HttpPost]
    public async Task<IActionResult> CreateVersionAsync(VersionInfo version)
    {
        _context.VersionInfos.Add(version);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // PUT: api/ComponentVersion/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVersionAsync(int id, VersionInfo version)
    {
        if (id != version.Id)
            return BadRequest("ID mismatch");

        var existing = await _context.VersionInfos.FindAsync(id);
        if (existing == null)
            return NotFound();

        // Update version fields and reset the Updated timestamp
        existing.ClientVersion = version.ClientVersion;
        existing.ApiVersion = version.ApiVersion;
        existing.WebAppVersion = version.WebAppVersion;
        existing.Updated = DateTime.UtcNow;

        _context.Entry(existing).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/ComponentVersion/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVersionAsync(int id)
    {
        var version = await _context.VersionInfos.FindAsync(id);
        if (version == null)
            return NotFound();

        _context.VersionInfos.Remove(version);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("~/api/v1/componentversion/avaterminal3")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLatestAvaTerminal3VersionAsync()
    {
        var latest = await _context.VersionInfos
            .Where(v => !string.IsNullOrEmpty(v.ClientVersion))
            .OrderByDescending(v => v.Updated)
            .FirstOrDefaultAsync();

        if (latest == null)
            return NotFound();

        return Ok(new { CurrentVersion = latest.ClientVersion });
    }

    // api version check
    [HttpGet("~/api/v1/componentversion/api")]
    [AllowAnonymous]
    public async Task<IActionResult> GetApiVersionAsync()
    {
        var latest = await _context.VersionInfos
            .Where(v => !string.IsNullOrEmpty(v.ApiVersion))
            .OrderByDescending(v => v.Updated)
            .FirstOrDefaultAsync();

        if (latest == null)
            return NotFound();

        return Ok(new { CurrentVersion = latest.ApiVersion });
    }

    // api health check
    [HttpGet("~/api/v1/componentversion/api/health")]
    [AllowAnonymous]
    public IActionResult GetApiHealthStatus() => Ok();
}
