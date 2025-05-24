namespace Ava.API.Helpers;

public class AmadeusUrlBuilder
{
    private readonly AmadeusSettings _settings;

    public AmadeusUrlBuilder(IOptions<AmadeusSettings> options)
    {
        _settings = options.Value;
    }

    public string BuildFlightOneWaySearchUrl(
        string originLocationCode,
        string destinationLocationCode,
        string departureDate,
        int adults,
        int children,
        int infants,
        string travelClass,
        string includedAirlineCodes,
        string excludedAirlineCodes,
        bool nonStop,
        string currencyCode,
        int maxPrice,
        int max)
    {
        // Get base URL from configuration
        string baseUrl = _settings.Url.FlightOffer;

        var parameters = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(originLocationCode)) parameters["originLocationCode"] = originLocationCode;
        if (!string.IsNullOrEmpty(destinationLocationCode)) parameters["destinationLocationCode"] = destinationLocationCode;
        if (!string.IsNullOrEmpty(departureDate)) parameters["departureDate"] = departureDate;
        if (!string.IsNullOrEmpty(travelClass)) parameters["travelClass"] = travelClass;
        if (!string.IsNullOrEmpty(includedAirlineCodes)) parameters["includedAirlineCodes"] = includedAirlineCodes;
        if (!string.IsNullOrEmpty(excludedAirlineCodes)) parameters["excludedAirlineCodes"] = excludedAirlineCodes;
        if (!string.IsNullOrEmpty(currencyCode)) parameters["currencyCode"] = currencyCode;

        if (adults > 0) parameters["adults"] = adults.ToString();
        if (children > 0) parameters["children"] = children.ToString();
        if (infants > 0) parameters["infants"] = infants.ToString();
        if (maxPrice > 0) parameters["maxPrice"] = maxPrice.ToString();
        if (max > 0) parameters["max"] = max.ToString();

        if (nonStop) parameters["nonStop"] = "true";

        string queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

        return $"{baseUrl}?{queryString}";
    }
}
