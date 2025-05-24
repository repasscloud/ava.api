namespace Ava.API.Services;

public class AvaLicenseGenerator : IAvaLicenseGenerator
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILoggerService _logger;

    public AvaLicenseGenerator(ApplicationDbContext context, IConfiguration configuration, ILoggerService logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateLicenseAsync(string clientId, DateTime expiryDate, string appId,
        int spendThreshold, string avaEmployeePrivateKey)
    {
        var _secretKey = _configuration["AvaLicense:SecretKey"];
        if (_secretKey is null)
        {
            await _logger.LogCriticalAsync("AvaLicense:SecretKey is missing from the configuration. Ensure it is set in appsettings.json or environment variables.");
            throw new Exception("AvaLicense:SecretKey is missing from the configuration. Ensure it is set in appsettings.json or environment variables.");
        }

        var avaEmployeeId = await _context.AvaEmployees
            .Where(x => x.PrivateKey == avaEmployeePrivateKey)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(avaEmployeeId))
        {
            await _logger.LogErrorAsync($"AvaEmployee not found for the given private key: {avaEmployeePrivateKey}");
            throw new Exception($"AvaEmployee not found for the given private key: {avaEmployeePrivateKey}");
        }

        // generate the license data
        var license = new AvaClientLicense
        {
            ClientID = clientId,
            ExpiryDate = expiryDate,
            AppID = appId,
            SpendThreshold = spendThreshold,
            IssuedBy = avaEmployeeId,
        };

        string data = JsonSerializer.Serialize(license);
        string signature = GenerateHmac(data, _secretKey);

        license.Signature = signature;

        // log messages
        await _logger.LogDebugAsync($"[AvaLicense] License successfully generated. Client ID: '{license.ClientID}', Expiry Date: {license.ExpiryDate:yyyy-MM-dd HH:mm:ss} UTC.");

        await _context.AvaClientLicenses.AddAsync(license);
        await _context.SaveChangesAsync();

        // log messages
        await _logger.LogDebugAsync($"[AvaLicense] License updated in DB. Client ID: '{license.ClientID}', Expiry Date: {license.ExpiryDate:yyyy-MM-dd HH:mm:ss} UTC.");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(license)));
    }

    private static string GenerateHmac(string data, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}
