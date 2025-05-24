namespace Ava.API.Controllers.Kernel;

[ApiController]
[Route("api/jwt-token-request")]
public class JwtTokenController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public JwtTokenController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/jwt-token-request
    [HttpPost]
    public async Task<ActionResult<AvaJwtTokenResponse>> CreateUser(ArcadiaAppMBServerJwtTokenRequest jwtTokenRequest)
    {
        if (jwtTokenRequest.Expires < DateTime.UtcNow)
        {
            return Ok(new AvaJwtTokenResponse { Id = 0, JwtToken = string.Empty, Expires = DateTime.UtcNow, IsValid = false });
        }

        // process ther request since hte token is still valid
        AvaJwtTokenResponse tokenResponse = new AvaJwtTokenResponse
        {
            Id = 0,
            JwtToken = jwtTokenRequest.JwtToken,
            Expires = jwtTokenRequest.Expires,
            IsValid = true
        };

        await _context.AvaJwtTokenResponses.AddAsync(tokenResponse);
        await _context.SaveChangesAsync();

        return Ok(tokenResponse);
    }

    // DELETE: api/jwt-token-request/{jwtToken}
    [HttpDelete("{jwtToken}")]
    public async Task<IActionResult> DeleteToken(string jwtToken)
    {
        var tokenRecord = await _context.AvaJwtTokenResponses
            .Where(jwt => jwt.JwtToken == jwtToken)
            .FirstOrDefaultAsync();

        if (tokenRecord == null) return NotFound();

        _context.AvaJwtTokenResponses.Remove(tokenRecord);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
