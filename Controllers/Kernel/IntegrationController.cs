namespace Ava.API.Controllers.Kernel;

[ApiController]
[Route("/api/v1/integration")]
public class IntegrationController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    private readonly ILoggerService _logger;

    public IntegrationController(IGitHubService gitHubService, ILoggerService loggerService)
    {
        _gitHubService = gitHubService;
        _logger = loggerService;
    }

    [HttpGet("github-credentials")]
    public async Task<ActionResult<List<GitHubRepoOAuthToken>>> GetAll()
    {
        var tokens = await _gitHubService.GetAllAsync();
        return Ok(tokens);
    }

    [HttpGet("github-credentials/{owner}/{repo}")]
    public async Task<ActionResult<GitHubRepoOAuthToken>> Get(string owner, string repo)
    {
        var token = await _gitHubService.GetAsync(owner, repo);
        if (token == null)
        {
            await _logger.LogWarningAsync($"Token not found for {owner}/{repo}.");
            return NotFound();
        }

        var tokenResponse = new GitHubRepoOAuthTokenDTO
        {
            Token = token.Token,
            Owner = token.Owner,
            Repo = token.Repo
        };

        return Ok(tokenResponse);
    }

    [HttpPost("github-credentials")]
    public async Task<ActionResult<GitHubRepoOAuthToken>> CreateOrUpdate(GitHubRepoOAuthTokenDTO token)
    {
        if (string.IsNullOrWhiteSpace(token.Owner) || string.IsNullOrWhiteSpace(token.Repo) || string.IsNullOrWhiteSpace(token.Token))
        {
            return BadRequest("Owner, Repo and Token fields are required.");
        }

        GitHubRepoOAuthToken oAuthToken = new GitHubRepoOAuthToken
        {
            Id = 0,
            Owner = token.Owner,
            Repo = token.Repo,
            Token = token.Token,
        };

        var result = await _gitHubService.CreateOrUpdateAsync(oAuthToken);
        return Ok(result);
    }

    [HttpDelete("github-credentials/{owner}/{repo}")]
    public async Task<IActionResult> Delete(string owner, string repo)
    {
        var deleted = await _gitHubService.DeleteAsync(owner, repo);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
