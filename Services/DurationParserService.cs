namespace Ava.API.Services;

/// <summary>
/// Provides methods for parsing ISO 8601 duration strings.
/// </summary>
public class DurationParserService : IDurationParserService
{
    /// <inheritdoc/>
    public string ParseIso8601Duration(string isoDuration)
    {
        if (string.IsNullOrEmpty(isoDuration))
            throw new ArgumentException("Duration string cannot be null or empty.", nameof(isoDuration));

        try
        {
            // Convert ISO 8601 duration to TimeSpan
            TimeSpan duration = XmlConvert.ToTimeSpan(isoDuration);

            // Build human-readable output
            string formattedDuration = $"{(duration.Hours > 0 ? $"{duration.Hours}h " : "")}" +
                                        $"{(duration.Minutes > 0 ? $"{duration.Minutes}m" : "")}".Trim();

            return string.IsNullOrEmpty(formattedDuration) ? "0m" : formattedDuration;
        }
        catch (FormatException ex)
        {
            throw new FormatException($"Invalid ISO 8601 duration format: {isoDuration}", ex);
        }
    }
}
