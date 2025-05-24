namespace Ava.API.Services;

public class AvaEmployeeService : IAvaEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILoggerService _loggerService;
    private readonly ICustomPasswordHasher _passwordHasher;
    private readonly IResend _resend;

    public AvaEmployeeService(ApplicationDbContext context, ILoggerService logger, ICustomPasswordHasher passwordHasher, IResend resend)
    {
        _context = context;
        _loggerService = logger;
        _passwordHasher = passwordHasher;
        _resend = resend;
    }

    public async Task<AvaEmployeeRecord?> GetByIdAsync(string id)
        => await _context.AvaEmployees.FindAsync(id);

    public async Task<AvaEmployeeRecord?> GetByIdOrEmailAsync(string emailOrId)
    {
        return await _context.AvaEmployees
            .FirstOrDefaultAsync(u => u.Id == emailOrId || u.Email == emailOrId.ToLower());
    }

    public async Task<List<AvaEmployeeRecord>> GetAllAsync()
        => await _context.AvaEmployees.ToListAsync();

    public async Task<AvaEmployeeRecord> CreateAsync(string firstName, string lastName, string email, bool isActive, string employeeType, string password, InternalRole role)
    {
        var employee = new AvaEmployeeRecord
        {
            Id = Nanoid.Generate(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLower(),
            PrivateKey = Nanoid.Generate(),
            CreatedAt = DateTime.UtcNow,
            IsActive = isActive,
            EmployeeType = employeeType,
            Role = role,
        };

        employee.PasswordHash = _passwordHasher.HashPassword(employee.PrivateKey, password);
        _context.AvaEmployees.Add(employee);
        await _context.SaveChangesAsync();
        await _loggerService.LogInfoAsync($"AvaEmployee with Id '{employee.Id}' and email '{employee.Email.ToLower()}' created successfully.");
        return employee;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _context.AvaEmployees.FindAsync(id);
        if (user is null) return false;

        _context.AvaEmployees.Remove(user);
        await _context.SaveChangesAsync();
        await _loggerService.LogInfoAsync($"AvaEmployee with Id '{user.Id}' has been deleted.");

        return true;
    }

    public async Task<bool> UpdateAsync(string id, AvaEmployeeUpdateDTO dto)
    {
        var user = await _context.AvaEmployees.FindAsync(id);
        if (user == null) return false;

        var hashedPassword = _passwordHasher.HashPassword(user.PrivateKey, dto.Password);

        if (hashedPassword == user.PasswordHash)
        {
            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            if (dto.IsActive.HasValue) user.IsActive = (bool)dto.IsActive;
            if (!string.IsNullOrWhiteSpace(dto.EmployeeType)) user.EmployeeType = dto.EmployeeType;
            if (dto.Role.HasValue) user.Role = dto.Role.Value;
            user.VerificationToken = null;

            await _loggerService.LogInfoAsync($"AvaEmployoee with Id '{user.Id}' was successfully updated.");
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> SetNewPasswordAsync(string id, string newPassword, string verificationToken)
    {
        var user = await _context.AvaEmployees.FindAsync(id);
        if (user is null) return false;

        if (!string.IsNullOrWhiteSpace(verificationToken) && !string.IsNullOrWhiteSpace(newPassword))
        {
            if (user.VerificationToken != verificationToken)
            {
                return false;
            }

            // clear password and verification token on user object
            user.VerificationToken = null;
            user.PasswordHash = null;

            // generate new user password hash
            user.PasswordHash = _passwordHasher.HashPassword(user.PrivateKey, newPassword);

            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public Task<bool> ImpersonateAsRoleAsync(string role)
    {
        // implementation for impersonation use-case (dummy / mocked as per app logic)
        return Task.FromResult(Enum.TryParse(typeof(InternalRole), role, true, out _));
    }

    public async Task<bool> ResetPasswordAsync(string id)
    {
        var lowered = id.ToLowerInvariant();
        var user = await _context.AvaEmployees.FirstOrDefaultAsync(u =>
            u.Id == id || u.Email.ToLower() == lowered);

        if (user is null)
        {
            await _loggerService.LogInfoAsync($"AvaEmployee with Id '{id}' was not found when requesting password reset.");
            return false;
        }

        user.PasswordHash = null;
        user.VerificationToken = Nanoid.Generate(size: 16);

        await _loggerService.LogInfoAsync($"AvaEmployee with Id '{id}' has requested password reset successfully.");
        await _context.SaveChangesAsync();

        await EmailVerificationCode(verificationCode: user.VerificationToken, receipientEmailAddress: user.Email.ToLowerInvariant());

        return true;
    }

    public async Task<bool> VerifyAccountAsync(AvaEmployeeVerifyAccountDTO dto)
    {
        var user = await _context.AvaEmployees.FirstOrDefaultAsync(u =>
            u.VerificationToken == dto.VerificationCode);

        if (user is null)
        {
            await _loggerService.LogInfoAsync($"AvaEmployee with VerificationToken '{dto.VerificationCode}' was not found during account verification.");
            return false;
        }

        // Clear the verification token to mark as verified
        user.VerificationToken = null;

        // Clear the user password hash to ensure it's not calculated in hashing
        user.PasswordHash = null;

        // Set the new password hash
        user.PasswordHash = _passwordHasher.HashPassword(user.PrivateKey, dto.Password);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task EmailVerificationCode(string verificationCode, string receipientEmailAddress)
    {
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset=""UTF-8"">
            <title>Password Reset Token</title>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    color: #333333;
                    background-color: #f9f9f9;
                    padding: 20px;
                }}
                .container {{
                    background-color: #ffffff;
                    border-radius: 8px;
                    padding: 30px;
                    max-width: 600px;
                    margin: auto;
                    box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                }}
                .token {{
                    font-size: 1.5em;
                    font-weight: bold;
                    background-color: #f1f1f1;
                    padding: 10px 20px;
                    border-radius: 6px;
                    display: inline-block;
                    margin: 20px 0;
                    letter-spacing: 2px;
                }}
                .footer {{
                    font-size: 0.9em;
                    color: #777777;
                    margin-top: 30px;
                }}
            </style>
            </head>
            <body>
            <div class=""container"">
                <h2>AvaAI Terminal2 Password Reset Request</h2>
                <p>Hello,</p>
                <p>We received a request to reset your password. To proceed, please open <strong>AvaAI Terminal2</strong> and navigate to the <code>AVF</code> route.</p>
                <p>When prompted, enter the verification token below:</p>

                <div class=""token"">{verificationCode}</div>

                <p>If you did not request this reset, please ignore this message.</p>

                <div class=""footer"">
                — AvaAI Support Team<br/>
                This token is valid for a limited time only.
                </div>
            </div>
            </body>
            </html>";

        var message = new EmailMessage();
        message.From = "AvaAI <no-reply@support.repasscloud.com>";
        message.To.Add(receipientEmailAddress);
        message.Subject = "AvaAI Terminal2 Password Reset";
        message.HtmlBody = htmlBody;

        var resp = await _resend.EmailSendAsync(message);
        await _loggerService.LogInfoAsync($"AvaEmployee verification code email send with Id '{resp.Content}'.");
    }

    public Task<bool> UpdatePasswordAsync(string id, string newPassword, string oldPassword)
    {
        throw new NotImplementedException();
    }
}
