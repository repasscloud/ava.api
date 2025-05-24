using Microsoft.Extensions.Logging;

namespace Ava.API.Services;

public class TPCityIATACodeService : ITPCityIATACodeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<TPCityIATACodeService> _logger;

    private const string ApiUrl = "https://api.travelpayouts.com/data/ru/cities.json?_gl=1*1aa4iag*_ga*MTY2NDk2Nzc2My4xNzM4MjkwMzI3*_ga_1WLL0NEBEH*MTczODQ5Nzg1OC4zLjEuMTczODQ5ODExNi42MC4wLjA.";

    public TPCityIATACodeService(IHttpClientFactory httpClientFactory, ApplicationDbContext context, Microsoft.Extensions.Logging.ILogger<TPCityIATACodeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _logger = logger;
    }

    public async Task SyncCitiesDataAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(ApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch city data: {response.StatusCode}");
                throw new Exception("Failed to retrieve city data from API.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var cities = JsonSerializer.Deserialize<List<TPCityIATACode>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (cities == null || cities.Count == 0)
            {
                _logger.LogWarning("No city data found in the API response.");
                return;
            }

            var existingCities = await _context.TPCityIATACodes.ToDictionaryAsync(c => c.Code);

            foreach (var city in cities)
            {
                if (existingCities.TryGetValue(city.Code, out var existingCity))
                {
                    // Update existing city record
                    existingCity.NameTranslations = city.NameTranslations;
                    existingCity.CityCode = city.CityCode;
                    existingCity.CountryCode = city.CountryCode;
                    existingCity.TimeZone = city.TimeZone;
                    existingCity.IataType = city.IataType;
                    existingCity.Name = city.Name;
                    existingCity.Coordinates = city.Coordinates;
                    existingCity.Flightable = city.Flightable;
                }
                else
                {
                    // Insert new city
                    _context.TPCityIATACodes.Add(city);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("City data successfully synchronized.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while syncing city data: {ex.Message}");
            throw;
        }
    }
}
