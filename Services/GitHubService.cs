namespace Ava.Shared.Services;

public class GitHubService : IGitHubService
{
    private readonly ApplicationDbContext _context;
    private readonly ILoggerService _logger;

    public GitHubService(ApplicationDbContext context, ILoggerService loggerService)
    {
        _context = context;
        _logger = loggerService;
    }

    public async Task<List<GitHubRepoOAuthToken>> GetAllAsync()
    {
        await _logger.LogInfoAsync("Fetching all GitHubRepoOAuthTokens.");
        return await _context.GitHubRepoOAuthTokens.ToListAsync();
    }

    public async Task<GitHubRepoOAuthToken?> GetAsync(string owner, string repo)
    {
        await _logger.LogDebugAsync($"Fetching GitHubRepoOAuthToken for {owner}/{repo}.");
        return await _context.GitHubRepoOAuthTokens
            .FirstOrDefaultAsync(x => x.Owner == owner && x.Repo == repo);
    }

    public async Task<GitHubRepoOAuthToken> CreateOrUpdateAsync(GitHubRepoOAuthToken token)
    {
        var existing = await _context.GitHubRepoOAuthTokens
            .FirstOrDefaultAsync(x => x.Owner == token.Owner && x.Repo == token.Repo);

        if (existing != null)
        {
            await _logger.LogInfoAsync($"Updating token for {token.Owner}/{token.Repo}.");
            existing.Token = token.Token;
            _context.GitHubRepoOAuthTokens.Update(existing);
        }
        else
        {
            await _logger.LogInfoAsync($"Creating new token for {token.Owner}/{token.Repo}.");
            await _context.GitHubRepoOAuthTokens.AddAsync(token);
        }

        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> DeleteAsync(string owner, string repo)
    {
        var token = await _context.GitHubRepoOAuthTokens
            .FirstOrDefaultAsync(x => x.Owner == owner && x.Repo == repo);

        if (token == null)
        {
            await _logger.LogWarningAsync($"Attempted to delete non-existent token for {owner}/{repo}.");
            return false;
        }

        _context.GitHubRepoOAuthTokens.Remove(token);
        await _context.SaveChangesAsync();
        await _logger.LogInfoAsync($"Deleted token for {owner}/{repo}.");
        return true;
    }
}
