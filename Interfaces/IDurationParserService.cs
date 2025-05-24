namespace Ava.API.Interfaces;

/// <summary>
/// Defines methods for parsing ISO 8601 duration strings.
/// </summary>
public interface IDurationParserService
{
    /// <summary>
    /// Parses an ISO 8601 duration string (e.g., "PT1H50M") into a human-readable format.
    /// </summary>
    /// <param name="isoDuration">The ISO 8601 duration string.</param>
    /// <returns>A formatted duration string (e.g., "1h 50m").</returns>
    /// <exception cref="ArgumentException">Thrown if the input string is null or empty.</exception>
    /// <exception cref="FormatException">Thrown if the input string is not in a valid ISO 8601 duration format.</exception>
    string ParseIso8601Duration(string isoDuration);
}
