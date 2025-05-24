namespace Ava.API.Helpers;

public static class TokenUtils
{
    public static string? GetBearerToken(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader["Bearer ".Length..].Trim();
        }

        return null;
    }
}
