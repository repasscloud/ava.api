namespace Ava.API.Services;

public class CustomPasswordHasher : ICustomPasswordHasher
{
    private readonly string _globalSalt;

    public CustomPasswordHasher(IConfiguration configuration)
    {
        _globalSalt = configuration["AvaSettings:GlobalSalt"]
                      ?? throw new InvalidOperationException("AvaSettings:GlobalSalt missing in configuration.");
    }

    public string HashPassword(string privateKey, string password)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new ArgumentException("privateKey must not be empty", nameof(privateKey));

        var combined = $"{password}:{privateKey}:{_globalSalt}";

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_globalSalt));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string privateKey, string password, string storedHash)
    {
        var computedHash = HashPassword(privateKey, password);
        return SlowEquals(storedHash, computedHash);
    }

    private static bool SlowEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        var diff = (uint)aBytes.Length ^ (uint)bBytes.Length;

        for (int i = 0; i < Math.Min(aBytes.Length, bBytes.Length); i++)
        {
            diff |= (uint)(aBytes[i] ^ bBytes[i]);
        }

        return diff == 0;
    }
}
