namespace Ava.API.Interfaces;
public interface IAvaLicenseGenerator
{
    Task<string> GenerateLicenseAsync(string clientId, DateTime expiryDate, string appId, int spendThreshold, string avaEmployeePrivateKey);
}
