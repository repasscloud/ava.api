namespace Ava.API.Services;

public class AmadeusAuthService : IAmadeusAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public AmadeusAuthService(IHttpClientFactory httpClientFactory, ApplicationDbContext context, IConfiguration configuration, JsonSerializerOptions jsonOptions)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _configuration = configuration;
        _jsonOptions = jsonOptions;
    }

    public async Task<AmadeusOAuthToken> GetTokenAsync()
    {
        var clientId = _configuration["Amadeus:ClientId"];
        if (clientId is null)
        {
            throw new Exception("");
        }
        
        var clientSecret = _configuration["Amadeus:ClientSecret"];
        if (clientSecret is null)
        {
            throw new Exception("");
        }
        var url = "https://test.api.amadeus.com/v1/security/oauth2/token";

        var requestData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var requestContent = new FormUrlEncodedContent(requestData);

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(url, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to retrieve token. Status Code: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var authServiceReponse = JsonSerializer.Deserialize<AmadeusOAuthResponse>(responseContent, _jsonOptions);

        if (authServiceReponse is not null)
        {
            AmadeusOAuthToken oAuthToken = new AmadeusOAuthToken
            {
                TokenType = authServiceReponse.TokenType,
                AccessToken = authServiceReponse.AccessToken,
                ExpiresIn = authServiceReponse.ExpiresIn,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.AmadeusOAuthTokens.Add(oAuthToken);
            await _context.SaveChangesAsync();

            return oAuthToken;
        }

        return new AmadeusOAuthToken();
    }

    public async Task<string> GetTokenInformationAsync()
    {
        var latestToken = await _context.AmadeusOAuthTokens
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestToken is not null && latestToken.ExpiryTime > DateTime.UtcNow)
        {
            return latestToken.AccessToken; // Return the valid token
        }

        return (await GetTokenAsync()).AccessToken; // Get a new token if the last one is expired
    }
}
