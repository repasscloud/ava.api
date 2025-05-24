namespace Ava.API.Interfaces;

public interface IAvaEmployeeService
{
    Task<AvaEmployeeRecord?> GetByIdAsync(string id);
    Task<AvaEmployeeRecord?> GetByIdOrEmailAsync(string emailOrId);
    Task<List<AvaEmployeeRecord>> GetAllAsync();
    Task<AvaEmployeeRecord> CreateAsync(string firstName, string lastName, string email, bool isActive, string employeeType, string password, InternalRole role);
    Task<bool> DeleteAsync(string id);
    Task<bool> UpdateAsync(string id, AvaEmployeeUpdateDTO dto);
    Task<bool> SetNewPasswordAsync(string id, string newPassword, string verificationToken);
    Task<bool> UpdatePasswordAsync(string id, string newPassword, string oldPassword);
    Task<bool> ResetPasswordAsync(string id);
    Task<bool> VerifyAccountAsync(AvaEmployeeVerifyAccountDTO dto);
    Task EmailVerificationCode(string verificationCode, string receipientEmailAddress);
    Task<bool> ImpersonateAsRoleAsync(string role);
}
