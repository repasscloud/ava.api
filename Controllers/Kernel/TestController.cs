using Ava.Shared.Extensions;

namespace Ava.API.Controllers.Kernel;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    public TestController(ApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    // USER
    [HttpPost("~/api/v1/test/user")]
    public IActionResult PostUser([FromBody] object payload)
    {

        return Ok(new { Message = "User POST received" });
    }


    [HttpGet("~/api/v1/test/user")]
    public async Task<IActionResult> GetUserAsync()
    {
        var jwt = HttpContext.GetJwtToken();

        if (!await _jwtTokenService.ValidateTokenAsync(jwt))
            return Unauthorized();

        var (issuer, audience, role) = await _jwtTokenService.ExtractClaimsFromBearerTokenAsync(HttpContext);

        if (issuer != "ava-api" || !AvaAudiences.Terminal.Contains(audience))
            return Unauthorized();

        return Ok(new
        {
            Message = "Token valid",
            Issuer = issuer,
            Audience = audience,
            Role = role
        });
    }

    [HttpGet("~/api/v1/test/user2")]
    public async Task<IActionResult> GetUser2Async()
    {
        var (isValid, errorResult, issuer, audience, role) = await _jwtTokenService.ValidateBearerTokenAsync(HttpContext);

        if (!isValid)
            return errorResult!;

        if (issuer != "ava-api" || !AvaAudiences.Terminal.Contains(audience))
            return Unauthorized();

        return Ok(new
        {
            Message = "Token valid",
            Issuer = issuer,
            Audience = audience,
            Role = role
        });
    }


    [HttpPut("~/api/v1/test/user")]
    public IActionResult PutUser([FromBody] object payload) => Ok("User PUT received");

    [HttpDelete("~/api/v1/test/user")]
    public IActionResult DeleteUser() => Ok("User DELETE received");

    // SYSTEM
    [HttpPost("~/api/v1/test/system")]
    public IActionResult PostSystem([FromBody] object payload) => Ok("System POST received");

    [HttpGet("~/api/v1/test/system")]
    public IActionResult GetSystem() => Ok("System GET received");

    [HttpPut("~/api/v1/test/system")]
    public IActionResult PutSystem([FromBody] object payload) => Ok("System PUT received");

    [HttpDelete("~/api/v1/test/system")]
    public IActionResult DeleteSystem() => Ok("System DELETE received");

    // EMPLOYEE
    [HttpPost("~/api/v1/test/employee")]
    public IActionResult PostEmployee([FromBody] object payload) => Ok("Employee POST received");

    [HttpGet("~/api/v1/test/employee")]
    public IActionResult GetEmployee() => Ok("Employee GET received");

    [HttpPut("~/api/v1/test/employee")]
    public IActionResult PutEmployee([FromBody] object payload) => Ok("Employee PUT received");

    [HttpDelete("~/api/v1/test/employee")]
    public IActionResult DeleteEmployee() => Ok("Employee DELETE received");

    // WEBAPP
    [HttpPost("~/api/v1/test/webapp")]
    public IActionResult PostWebApp([FromBody] object payload) => Ok("WebApp POST received");

    [HttpGet("~/api/v1/test/webapp")]
    public IActionResult GetWebApp() => Ok("WebApp GET received");

    [HttpPut("~/api/v1/test/webapp")]
    public IActionResult PutWebApp([FromBody] object payload) => Ok("WebApp PUT received");

    [HttpDelete("~/api/v1/test/webapp")]
    public IActionResult DeleteWebApp() => Ok("WebApp DELETE received");

    [Authorize]
    [HttpGet("~/api/v1/test/auth/claims")]
    public IActionResult Claims()
    {
        return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("~/api/v1/test/auth/isadmin")]
    public IActionResult AdminOnly() => Ok(new { Message = "You are Admin ✅", Role = "Admin", Timestamp = DateTime.UtcNow });
}
