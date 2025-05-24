namespace Ava.API.Interfaces;

public interface IGitHubService
{
    Task<List<GitHubRepoOAuthToken>> GetAllAsync();
    Task<GitHubRepoOAuthToken?> GetAsync(string owner, string repo);
    Task<GitHubRepoOAuthToken> CreateOrUpdateAsync(GitHubRepoOAuthToken token);
    Task<bool> DeleteAsync(string owner, string repo);
}
