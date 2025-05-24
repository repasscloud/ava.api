namespace Ava.API.Services;

public class AvaLicenseValidator : IAvaLicenseValidator
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILoggerService _logger;

    public AvaLicenseValidator(ApplicationDbContext context, IConfiguration configuration, ILoggerService logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool isValid, AvaClientLicense? license)> ValidateLicenseAsync(string encodedLicense)
    {
        var _secretKey = _configuration["AvaLicense:SecretKey"];
        if (_secretKey is null)
        {
            await _logger.LogCriticalAsync("AvaLicense:SecretKey is missing from the configuration. Ensure it is set in appsettings.json or environment variables.");
            throw new Exception("AvaLicense:SecretKey is missing from the configuration. Ensure it is set in appsettings.json or environment variables.");
        }

        try
        {
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(encodedLicense));
            var license = JsonSerializer.Deserialize<AvaClientLicense>(json);

            if (license == null)
            {
                await _logger.LogInfoAsync("License deserialization failed: The provided license data could not be parsed.");
                return (false, null);
            }

            if (license.ExpiryDate < DateTime.UtcNow)
            {
                await _logger.LogInfoAsync($"License validation failed: The license expired on {license.ExpiryDate:yyyy-MM-dd HH:mm:ss} UTC.");
                return (false, null);
            }
                

            string originalData = JsonSerializer.Serialize(new
            {
                license.ClientID,
                license.ExpiryDate,
                license.AppID
            });

            bool isValid = license.Signature == GenerateHmac(originalData, _secretKey);
            return (isValid, isValid ? license : null);
        }
        catch
        {
            return (false, null);
        }
    }

    private static string GenerateHmac(string data, string _secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
