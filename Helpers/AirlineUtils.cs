namespace Ava.API.Helpers;

public static class AirlineUtils
{
    public static string NormalizeAirlineCodes(string? airlineCodes)
    {
        if (string.IsNullOrWhiteSpace(airlineCodes))
        {
            return string.Empty;
        }

        return string.Join(",", airlineCodes
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)  // split by space or comma
            .Select(code => code.Trim().ToUpperInvariant())  // trim whitespace and make capitilised
            .Distinct());  // remove duplicates
    }

    public static string GetTravelClassName(int value)
    {
        if (Enum.IsDefined(typeof(FlightTravelClassType), value))
        {
            return Enum.GetName(typeof(FlightTravelClassType), value) ?? "ECONOMY";
        }
        return "ECONOMY";  // handle invalid values gracefully
    }
}
