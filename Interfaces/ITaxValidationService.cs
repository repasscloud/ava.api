namespace Ava.API.Interfaces;

public interface ITaxValidationService
{
    Task<DateTime?> ValidateTaxRegistrationAsync(string taxRegistrationId, string country);
}