using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using SmartFintechFinancial.Modules.Identity.Application.Services;
using SmartFintechFinancial.Modules.Identity.Application.Settings;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IBruteForceProtection _bruteForceProtection;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ISSRFProtectionService _ssrfProtectionService;
    private readonly AuthSettings _authSettings;

    public AuthService(
        IdentityDbContext context,
        ILogger<AuthService> logger,
        IBruteForceProtection bruteForceProtection,
        IRateLimitingService rateLimitingService,
        ISSRFProtectionService ssrfProtectionService,
        IOptions<AuthSettings> authSettings)
    {
        _context = context;
        _logger = logger;
        _bruteForceProtection = bruteForceProtection;
        _rateLimitingService = rateLimitingService;
        _ssrfProtectionService = ssrfProtectionService;
        _authSettings = authSettings.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string ipAddress)
    {
        try
        {
            // Rate limiting for registration
            if (await _rateLimitingService.IsRateLimitedAsync("register", ipAddress))
            {
                return new AuthResult(false, Error: "Too many registration attempts. Please try again later.");
            }

            await _rateLimitingService.RecordRequestAsync("register", ipAddress);

            // Check for brute force attacks on registration
            if (await _bruteForceProtection.IsAccountLockedAsync(request.Email))
            {
                return new AuthResult(false, Error: "Account is temporarily locked. Please try again later.");
            }

            // Input sanitization
            if (!IsValidInput(request.Email) || !IsValidInput(request.Password))
            {
                await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                _logger.LogWarning("Potential injection attempt during registration from IP: {IP}", ipAddress);
                return new AuthResult(false, Error: "Invalid input detected");
            }

            // Check if email exists with transaction to prevent race conditions
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                if (await EmailExistsAsync(request.Email))
                {
                    await transaction.RollbackAsync();
                    return new AuthResult(false, Error: "Email already registered");
                }

                // Create user with audit trail - ✅ REMOVED manual audit fields
                var user = new User
                {
                    Email = request.Email.ToLowerInvariant(),
                    PasswordHash = User.HashPassword(request.Password),
                    Phone = SanitizePhoneNumber(request.Phone),
                    // ❌ REMOVED: CreatedBy, CreatedAt - they auto-populate
                    // ❌ REMOVED: LastLoginAt - stays null initially
                    IsActive = true,
                    EmailVerified = false // Will require email verification
                    // ❌ REMOVED: FailedLoginAttempts - defaults to 0
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // ✅ This sets CreatedAt, CreatedBy
                await transaction.CommitAsync();

                // Reset any failed attempts for this email
                await _bruteForceProtection.ResetFailedAttemptsAsync(request.Email);

                _logger.LogInformation("User registered successfully: {Email} from IP: {IP}",
                    SanitizeForLog(request.Email), ipAddress);

                // TODO: Send email verification

                return new AuthResult(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (ex.InnerException?.Message?.Contains("unique constraint") == true ||
                    ex.InnerException?.Message?.Contains("duplicate key") == true)
                {
                    // Race condition occurred
                    return new AuthResult(false, Error: "Email already registered");
                }

                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return new AuthResult(false, Error: "Registration failed");
        }
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
    {
        try
        {
            // Rate limiting for login
            if (await _rateLimitingService.IsRateLimitedAsync("login", ipAddress))
            {
                return new AuthResult(false, Error: "Too many login attempts. Please try again later.");
            }

            await _rateLimitingService.RecordRequestAsync("login", ipAddress);

            // Check for brute force protection
            if (_authSettings.EnableBruteForceProtection)
            {
                if (await _bruteForceProtection.IsAccountLockedAsync(request.Email))
                {
                    return new AuthResult(false,
                        Error: "Account is temporarily locked due to too many failed attempts.",
                        AccountLocked: true);
                }
            }

            // Input sanitization
            if (!IsValidInput(request.Email) || !IsValidInput(request.Password))
            {
                if (_authSettings.EnableBruteForceProtection)
                {
                    await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                }

                _logger.LogWarning("Potential injection attempt during login for email: {Email} from IP: {IP}",
                    SanitizeForLog(request.Email), ipAddress);

                // Use consistent error message to prevent user enumeration
                await Task.Delay(2000); // Artificial delay to slow down attacks
                return new AuthResult(false, Error: "Invalid email or password");
            }

            // Find user with case-insensitive email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

            if (user == null)
            {
                // User not found - still record failed attempt
                if (_authSettings.EnableBruteForceProtection)
                {
                    await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                }

                await Task.Delay(2000); // Artificial delay
                return new AuthResult(false, Error: "Invalid email or password");
            }

            // Check if account is active
            if (!user.IsActive)
            {
                return new AuthResult(false, Error: "Account is deactivated. Please contact support.");
            }

            // Verify password with timing attack protection
            var passwordValid = user.VerifyPassword(request.Password);

            if (!passwordValid)
            {
                if (_authSettings.EnableBruteForceProtection)
                {
                    await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                    var failedAttempts = await _bruteForceProtection.GetFailedAttemptsCountAsync(request.Email);

                    // ✅ Update user's failed login attempts - keep this
                    user.FailedLoginAttempts++;
                    // ❌ REMOVED manual UpdatedAt/UpdatedBy - they auto-populate

                    await _context.SaveChangesAsync(); // ✅ This sets UpdatedAt/UpdatedBy

                    return new AuthResult(false,
                        Error: "Invalid email or password",
                        FailedAttempts: failedAttempts,
                        AccountLocked: failedAttempts >= _authSettings.MaxFailedLoginAttempts);
                }

                await Task.Delay(2000); // Artificial delay
                return new AuthResult(false, Error: "Invalid email or password");
            }

            // Check for 2FA if enabled
            if (!string.IsNullOrEmpty(request.TwoFactorCode))
            {
                // TODO: Implement 2FA verification
                _logger.LogInformation("2FA code provided for {Email}: {Code}",
                    SanitizeForLog(request.Email), request.TwoFactorCode);
            }

            // Successful login - reset failed attempts
            if (_authSettings.EnableBruteForceProtection)
            {
                await _bruteForceProtection.ResetFailedAttemptsAsync(request.Email);
            }

            //  Update user login information - keep these business fields
            user.LastLoginAt = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            //  REMOVED UpdatedAt/UpdatedBy - they auto-populate

            // Log successful login
            await LogSuccessfulLogin(user.Id, ipAddress, userAgent);

            await _context.SaveChangesAsync(); //  This sets UpdatedAt/UpdatedBy

            _logger.LogInformation("User logged in successfully: {Email} from IP: {IP}",
                SanitizeForLog(user.Email), ipAddress);

            return new AuthResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return new AuthResult(false, Error: "Login failed");
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (!IsValidInput(email))
            return false;

        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        if (!IsValidInput(email) || !IsValidInput(password))
            return false;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        return user != null && user.VerifyPassword(password);
    }

    public async Task<bool> LockAccountAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        if (user == null)
            return false;

        user.IsActive = false;
        //  REMOVED UpdatedAt/UpdatedBy - they auto-populate

        await _context.SaveChangesAsync(); //  This sets UpdatedAt/UpdatedBy
        return true;
    }

    public async Task<bool> UnlockAccountAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

        if (user == null)
            return false;

        user.IsActive = true;
        user.FailedLoginAttempts = 0;
        //  REMOVED UpdatedAt/UpdatedBy - they auto-populate

        await _context.SaveChangesAsync(); //  This sets UpdatedAt/UpdatedBy
        return true;
    }

    #region Helper Methods

    private bool IsValidInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // SQL Injection patterns
        var sqlPatterns = new[]
        {
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|EXEC|EXECUTE|ALTER|CREATE|TRUNCATE|MERGE|BEGIN|END|DECLARE)\b)",
            @"(\-\-|\#|\/\*|\*\/)",
            @"(\b(OR|AND)\b\s+\d+\s*=\s*\d+)",
            @"(WAITFOR\s+DELAY\s+'[^']+')",
            @"(\b(XP_|SP_|FN_|MSys|SYSOBJECTS|SYSUSERS)\w*\b)",
            @"(EXEC\s*\(\s*@)",
            @"(CHAR\(\d+\)\+CHAR\(\d+\))",
            @"(;|\'|\""|\\|--|#|/\\*|\\*/)"
        };

        foreach (var pattern in sqlPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(
                input, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        // Check for XSS patterns
        var xssPatterns = new[]
        {
            @"<script.*?>.*?</script>",
            @"javascript:",
            @"on\w+\s*=",
            @"<iframe.*?>.*?</iframe>",
            @"<object.*?>.*?</object>",
            @"<embed.*?>.*?</embed>"
        };

        foreach (var pattern in xssPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(
                input, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private string SanitizePhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return null;

        // Remove all non-numeric characters except plus sign
        var sanitized = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d+]", "");

        // Validate E.164 format
        if (!System.Text.RegularExpressions.Regex.IsMatch(sanitized, @"^\+?[1-9]\d{1,14}$"))
        {
            return null;
        }

        return sanitized;
    }

    private async Task LogSuccessfulLogin(Guid userId, string ipAddress, string userAgent)
    {
        try
        {
            var loginLog = new LoginLog
            {
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccess = true
                //  CreatedAt will be auto-set by SaveChanges
            };

            _context.LoginLogs.Add(loginLog);
            await _context.SaveChangesAsync(); //  This sets audit fields
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log successful login for user {UserId}", userId);
        }
    }

    private string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "[empty]";

        return System.Text.RegularExpressions.Regex.Replace(
            input, @"[^\w\s@\.\-]", "[X]",
            System.Text.RegularExpressions.RegexOptions.None,
            TimeSpan.FromMilliseconds(100));
    }

    #endregion
}