namespace Ava.API.Controllers.Kernel;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class ComponentVersionController : ControllerBase
{
    [HttpGet("~/api/v1/avaterminal3version")]
    public IActionResult GetVersionAsync()
    {
        return Ok(new { CurrentVersion = VersionInfo.ClientVersion });
    }
}
