namespace Ava.API.Interfaces;

public interface IAvaLicenseValidator
{
    Task<(bool isValid, AvaClientLicense? license)> ValidateLicenseAsync(string encodedLicense);
}
