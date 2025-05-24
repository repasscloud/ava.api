namespace Ava.API.Interfaces;

public interface ICustomPasswordHasher
{
    string HashPassword(string privateKey, string password);
    bool VerifyPassword(string privateKey, string password, string storedHash);
}
