namespace Ava.API.Controllers.Kernel;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AvaEmployeeAuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAvaEmployeeService _avaEmployeeService;
    private readonly ApplicationDbContext _context;
    private readonly ICustomPasswordHasher _passwordHasher;
    private readonly ILoggerService _loggerService;
    private readonly IConfiguration _configuration;

    public AvaEmployeeAuthController(
        IJwtTokenService jwtTokenService,
        IAvaEmployeeService avaEmployeeService,
        ApplicationDbContext context,
        ICustomPasswordHasher passwordHasher,
        ILoggerService loggerService,
        IConfiguration configuration)
    {
        _jwtTokenService = jwtTokenService;
        _avaEmployeeService = avaEmployeeService;
        _context = context;
        _passwordHasher = passwordHasher;
        _loggerService = loggerService;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("~/api/v1/auth/login")]
    public async Task<IActionResult> Login([FromBody] AvaEmployeeLoginDTO dto)
    {
        var user = await _avaEmployeeService.GetByIdOrEmailAsync(dto.Username);

        if (user is null)
            return Unauthorized("User not found.");

        var jwtIssuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer is missing");

        var jwtAudience = _configuration["JwtSettings:Audiences:1"]
            ?? throw new InvalidOperationException("JwtSettings:Audiences:1 is missing");

        if (!string.IsNullOrEmpty(user.PasswordHash) &&
            _passwordHasher.VerifyPassword(user.PrivateKey, dto.Password, user.PasswordHash))
        {
            // expiry minutes
            int expiryMins = 480;
            // create JWT Token now
            var token = await _jwtTokenService.GenerateTokenAsync(
                userId: user.Id,
                username: user.Email,
                role: user.Role.ToString(),
                audience: jwtAudience,
                issuer: jwtIssuer,
                expiryMinutes: expiryMins
            );

            // create an AvaJwtTokenResponse for quick verification
            var savedToken = new AvaJwtTokenResponse()
            {
                Id = 0,
                JwtToken = token,
                Expires = DateTime.UtcNow.AddMinutes(expiryMins),
                IsValid = true,
            };

            // save the token to the DB
            var result = await _jwtTokenService.SaveTokenToDbAsync(savedToken);
            if (result)
            {
                return Ok(new AvaEmployeeLoginResponseDTO { Token = token });
            }
            else
            {
                return BadRequest("Token not saved to db.");
            }
        }

        return Unauthorized("Password incorrect.");
    }

    [AllowAnonymous]
    [HttpPost("~/api/v1/auth/register")]
    public async Task<IActionResult> Register([FromBody] AvaEmployeeRegisterDTO dto)
    {
        // var token = TokenUtils.GetBearerToken(HttpContext);
        // if (token == null)
        // {
        //     await _loggerService.LogErrorAsync($"Missing Bearer token for creating user with email '{dto.Email}'.");
        //     return BadRequest("Missing Bearer token.");
        // }

        // var existingToken = await _context.AvaJwtTokenResponses
        //     .FirstOrDefaultAsync(x => x.JwtToken == token);

        // if (existingToken == null)
        // {
        //     await _loggerService.LogErrorAsync($"Token not found for creating user with email '{dto.Email}'.");
        //     return BadRequest("Token not found.");
        // }

        // if (!existingToken.IsValid)
        // {
        //     await _loggerService.LogErrorAsync($"Token is marked as invalid for creating user with email '{dto.Email}'.");
        //     return BadRequest("Token is marked as invalid.");
        // }

        // if (existingToken.Expires <= DateTime.UtcNow)
        // {
        //     await _loggerService.LogErrorAsync($"Token '{token}' has expired for creating user with email '{dto.Email}'.");
        //     return BadRequest("Token '{token}' has expired for creating user with email '{dto.Email}'.");
        // }

        // create the new user object          
        var newUser = new AvaEmployeeRecord()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.ToLowerInvariant(),
            EmployeeType = dto.EmployeeType,
            VerificationToken = Nanoid.Generate(size: 18),
            Role = dto.Role,
        };

        await _context.AvaEmployees.AddAsync(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { newUser.VerificationToken });
    }

    [AllowAnonymous]
    [HttpPost("~/api/v1/auth/reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDTO dto)
        => (await _avaEmployeeService.ResetPasswordAsync(dto.Email))
            ? Ok("If an account exists for the provided information, a password reset has been initiated.")
            : Ok("If an account exists for the provided information, a password reset has been initiated.");

    [Authorize(Roles = "Admin")]
    [HttpDelete("~/api/v1/auth/{id}")]
    public async Task<IActionResult> Delete(string id)
        => (await _avaEmployeeService.DeleteAsync(id)) ? Ok() : NotFound();

    [HttpPut("~/api/v1/auth/{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AvaEmployeeUpdateDTO dto)
        => (await _avaEmployeeService.UpdateAsync(id, dto)) ? Ok() : NotFound();

    [AllowAnonymous]
    [HttpPost("~/api/v1/auth/verify-account")]
    public async Task<IActionResult> VerifyAccount([FromBody] AvaEmployeeVerifyAccountDTO dto)
        => (await _avaEmployeeService.VerifyAccountAsync(dto)) ? Ok() : NotFound();

    [HttpPost("~/api/v1/auth/logout")]
    public async Task<IActionResult> LogoutAccount()
    {
        var jwt = HttpContext.GetJwtToken();
        if (jwt == null)
        {
            return BadRequest();
        }

        return await _jwtTokenService.LogoutAsync(jwt) ? Ok() : NotFound();
    }

    [Authorize(Roles = "System")]
    [HttpGet("~/api/v1/auth/logout-all-users")]
    public async Task<IActionResult> LogoutAllUsersAsync()
    {
        await _jwtTokenService.ClearAllTokensAsync();
        return Ok();
    }
}
