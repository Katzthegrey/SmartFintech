using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using SmartFintechFinancial.Modules.Identity.Application.Settings;
using SmartFintechFinancial.Modules.Identity.Domain.Constants;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;
using SmartGuardFinancial.Modules.Identity.Application.Services;
using System.Diagnostics;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IBruteForceProtection _bruteForceProtection;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ISSRFProtectionService _ssrfProtectionService;
    private readonly AuthSettings _authSettings;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IdentityDbContext context,
        ILogger<AuthService> logger,
        IBruteForceProtection bruteForceProtection,
        IRateLimitingService rateLimitingService,
        ISSRFProtectionService ssrfProtectionService,
        IOptions<AuthSettings> authSettings,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _logger = logger;
        _bruteForceProtection = bruteForceProtection;
        _rateLimitingService = rateLimitingService;
        _ssrfProtectionService = ssrfProtectionService;
        _authSettings = authSettings.Value;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string ipAddress)
    {
        Debug.WriteLine("==========================================");
        Debug.WriteLine($"REGISTERASYNC CALLED at {DateTime.UtcNow:HH:mm:ss.fff}");
        Debug.WriteLine($"Email: {request.Email}");
        Debug.WriteLine($"IP: {ipAddress}");
        Debug.WriteLine("==========================================");

        try
        {
            // Rate limiting for registration
            if (await _rateLimitingService.IsRateLimitedAsync("register", ipAddress))
            {
                Debug.WriteLine("Rate limited - too many attempts");
                return new AuthResult(false, Error: "Too many registration attempts. Please try again later.");
            }

            await _rateLimitingService.RecordRequestAsync("register", ipAddress);

            // Check for brute force attacks on registration
            if (await _bruteForceProtection.IsAccountLockedAsync(request.Email))
            {
                Debug.WriteLine("Account locked");
                return new AuthResult(false, Error: "Account is temporarily locked. Please try again later.");
            }

            // Input sanitization
            if (!IsValidInput(request.Email) || !IsValidInput(request.Password))
            {
                Debug.WriteLine("Invalid input detected - possible injection attempt");
                await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                _logger.LogWarning("Potential injection attempt during registration from IP: {IP}", ipAddress);
                return new AuthResult(false, Error: "Invalid input detected");
            }

            // Check if passwords match
            if (request.Password != request.ConfirmPassword)
            {
                Debug.WriteLine("Passwords do not match");
                return new AuthResult(false, Error: "Passwords do not match");
            }

            // Use execution strategy for ALL database operations
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Start transaction inside the strategy
                using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                try
                {
                    Debug.WriteLine("Checking if email already exists...");
                    // EmailExistsAsync is now INSIDE the execution strategy
                    if (await EmailExistsAsync(request.Email))
                    {
                        Debug.WriteLine("Email already exists");
                        await transaction.RollbackAsync();
                        return new AuthResult(false, Error: "Email already registered");
                    }

                    Debug.WriteLine("Email is available, creating user...");

                    // Create user with audit trail 
                    var user = new User
                    {
                        Email = request.Email.ToLowerInvariant(),
                        PasswordHash = User.HashPassword(request.Password),
                        Phone = SanitizePhoneNumber(request.Phone),
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        DateOfBirth = EnsureUtc(request.DateOfBirth),
                        Address = request.Address,
                        City = request.City,
                        PostalCode = request.PostalCode,
                        Country = request.Country,
                        AnnualIncome = request.AnnualIncome,
                        EmploymentStatus = request.EmploymentStatus,
                        SourceOfFunds = request.SourceOfFunds,
                        TaxIdNumber = request.TaxIdNumber,
                        IsActive = true,
                        EmailVerified = false,
                        KycStatus = KycStatus.Pending,
                        RiskLevel = RiskLevel.Low,
                        SubscriptionTier = "Free",
                        Currency = request.Currency ?? "ZAR",
                        Language = request.Language ?? "en",
                        InvestmentRiskTolerance = request.InvestmentRiskTolerance ?? RiskTolerance.Moderate,
                        PrimaryInvestmentGoal = request.PrimaryInvestmentGoal ?? InvestmentGoal.Growth,
                        PreferredInvestmentTypes = request.PreferredInvestmentTypes,
                        ConsentGiven = request.ConsentGiven ?? false,
                        ConsentGivenAt = request.ConsentGiven == true ? DateTime.UtcNow : null,
                        MarketingOptIn = request.MarketingOptIn ?? false,
                        ConsentPreferences = request.ConsentPreferences,
                        CreatedBy = "registration"
                    };

                    Debug.WriteLine($"User object created:");
                    Debug.WriteLine($"  Id: {user.Id}");
                    Debug.WriteLine($"  Email: {user.Email}");
                    Debug.WriteLine($"  FirstName: {user.FirstName}");
                    Debug.WriteLine($"  LastName: {user.LastName}");
                    Debug.WriteLine($"  KycStatus: {user.KycStatus}");
                    Debug.WriteLine($"  Currency: {user.Currency}");
                    Debug.WriteLine($"  Language: {user.Language}");

                    // After creating the user object, before saving
                    Debug.WriteLine("Assigning default role...");

                    // Map registration type to role name
                    string roleName = request.RegistrationType switch
                    {
                        "client" => RoleConstants.Client,                    // Basic client
                        "investor" => RoleConstants.Investor,                // Investment account holder
                        "premium" => RoleConstants.PremiumInvestor,          // High-value investor
                        "business" => RoleConstants.BusinessInvestor,        // Business/Corporate investor
                        "advisor" => RoleConstants.FinancialAdvisor,         // Registered financial advisor
                        "wealth-manager" => RoleConstants.WealthManager,     // Portfolio/wealth manager
                        "support" => RoleConstants.SupportAgent,             // Customer support
                        "fraud-analyst" => RoleConstants.FraudAnalyst,       // Fraud detection specialist
                        "compliance" => RoleConstants.ComplianceOfficer,     // Regulatory compliance
                        "finance-admin" => RoleConstants.FinanceAdmin,       // Financial operations admin
                        "admin" => RoleConstants.SuperAdmin,                  // Full system access
                        _ => RoleConstants.Client  // Default fallback
                    };

                    Debug.WriteLine($"Registration type '{request.RegistrationType}' maps to role '{roleName}'");

                    // Get the role from database
                    var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == roleName);

                    if (role != null)
                    {
                        user.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                        Debug.WriteLine($"Role '{role.Name}' assigned to user based on registration type: {request.RegistrationType}");
                    }
                    else
                    {
                        Debug.WriteLine($"WARNING: Role '{roleName}' not found in database! Falling back to Client role.");
                        _logger.LogWarning("Role '{RoleName}' not found for registration type '{RegType}', falling back to Client",
                            roleName, request.RegistrationType);

                        // Fallback to Client role
                        var clientRole = await _context.Roles
                            .FirstOrDefaultAsync(r => r.Name == RoleConstants.Client);

                        if (clientRole != null)
                        {
                            user.UserRoles.Add(new UserRole
                            {
                                UserId = user.Id,
                                RoleId = clientRole.Id,
                                AssignedAt = DateTime.UtcNow
                            });
                            Debug.WriteLine("Fallback: Client role assigned instead");
                        }
                        else
                        {
                            Debug.WriteLine("CRITICAL: Even Client role not found in database!");
                            _logger.LogCritical("Client role not found in database during registration!");
                        }
                    }

                    _context.Users.Add(user);

                    Debug.WriteLine("Attempting to save user to database...");
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("User saved successfully!");

                    Debug.WriteLine("Loading roles for token generation...");

                    // Explicitly load the user's roles from the database
                    await _context.Entry(user)
                        .Collection(u => u.UserRoles)
                        .Query()
                        .Include(ur => ur.Role)
                        .LoadAsync();

                    Debug.WriteLine($"Roles loaded: {user.UserRoles?.Count ?? 0} roles found");
                    foreach (var userRole in user.UserRoles ?? new List<UserRole>())
                    {
                        Debug.WriteLine($"  - Role: {userRole.Role?.Name}");
                    }

                    // Generate tokens
                    Debug.WriteLine("Generating tokens...");
                    var accessToken = _tokenService.GenerateAccessToken(user);
                    var refreshToken = _tokenService.GenerateRefreshToken(ipAddress, user.Id);
                    refreshToken.UserId = user.Id;

                    _context.RefreshTokens.Add(refreshToken);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Tokens saved successfully!");

                    await transaction.CommitAsync();
                    Debug.WriteLine("Transaction committed!");

                    // Reset any failed attempts for this email
                    await _bruteForceProtection.ResetFailedAttemptsAsync(request.Email);

                    _logger.LogInformation("User registered successfully: {Email} from IP: {IP}",
                        SanitizeForLog(request.Email), ipAddress);

                    // Determine next steps based on registration type
                    var nextStep = request.RegistrationType switch
                    {
                        "investor" => "complete-kyc",
                        "advisor" => "verify-credentials",
                        "business" => "verify-business",
                        _ => "verify-email"
                    };

                    Debug.WriteLine("=== REGISTRATION COMPLETE ===");

                    return new AuthResult(
                        true,
                        AccessToken: accessToken,
                        RefreshToken: refreshToken.Token,
                        ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                        RequiresTwoFactor: false,
                        NextStep: nextStep,
                        Message: "Registration successful. Please check your email for verification."
                    );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("=== DATABASE ERROR DURING SAVE ===");
                    Debug.WriteLine($"Exception type: {ex.GetType().Name}");
                    Debug.WriteLine($"Message: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        Debug.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                        Debug.WriteLine($"Inner message: {ex.InnerException.Message}");

                        // Check for PostgreSQL specific error
                        if (ex.InnerException is Npgsql.PostgresException pgEx)
                        {
                            Debug.WriteLine($"PostgreSQL Error Code: {pgEx.SqlState}");
                            Debug.WriteLine($"Error Message: {pgEx.MessageText}");
                            Debug.WriteLine($"Detail: {pgEx.Detail}");
                            Debug.WriteLine($"Table: {pgEx.TableName}");
                            Debug.WriteLine($"Column: {pgEx.ColumnName}");
                            Debug.WriteLine($"Constraint: {pgEx.ConstraintName}");
                        }
                    }

                    Debug.WriteLine("Rolling back transaction...");
                    await transaction.RollbackAsync();

                    if (ex.InnerException?.Message?.Contains("unique constraint") == true ||
                        ex.InnerException?.Message?.Contains("duplicate key") == true)
                    {
                        Debug.WriteLine("Duplicate key/unique constraint violation");
                        return new AuthResult(false, Error: "Email already registered");
                    }

                    throw; // Let the strategy handle retries if needed
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("=== TOP LEVEL EXCEPTION ===");
            Debug.WriteLine($"Type: {ex.GetType().Name}");
            Debug.WriteLine($"Message: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            _logger.LogError(ex, "Error during registration for {Email}", request.Email);

            // Don't return generic "Registration failed" - be more specific when safe
            if (ex is DbUpdateException dbEx && dbEx.InnerException?.Message?.Contains("unique constraint") == true)
            {
                return new AuthResult(false, Error: "Email already registered");
            }

            if (ex is OperationCanceledException)
            {
                return new AuthResult(false, Error: "Request timed out. Please try again.");
            }

            // Rate limiting errors should already be caught earlier
            return new AuthResult(false, Error: "Registration service temporarily unavailable");
        }
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, string userAgent)
    {
        _logger.LogInformation("=== LOGINASYNC STARTED ===");
        _logger.LogInformation("Email: {Email}", request?.Email ?? "NULL");
        _logger.LogInformation("IP Address: {IP}", ipAddress);
        _logger.LogInformation("User Agent: {UserAgent}", userAgent);
        _logger.LogInformation("RequireVerifiedKyc: {RequireKyc}", request?.RequireVerifiedKyc ?? false);

        try
        {
            // Rate limiting for login
            _logger.LogInformation("Step 1: Checking rate limit for IP: {IP}", ipAddress);
            if (await _rateLimitingService.IsRateLimitedAsync("login", ipAddress))
            {
                _logger.LogWarning("Rate limit exceeded for IP: {IP}", ipAddress);
                return new AuthResult(false, Error: "Too many login attempts. Please try again later.");
            }
            _logger.LogInformation("Rate limit check passed");

            await _rateLimitingService.RecordRequestAsync("login", ipAddress);
            _logger.LogInformation("Rate limit request recorded");

            // Check for brute force protection
            if (_authSettings.EnableBruteForceProtection)
            {
                _logger.LogInformation("Step 2: Checking brute force protection for email: {Email}", request.Email);
                var isLocked = await _bruteForceProtection.IsAccountLockedAsync(request.Email);
                _logger.LogInformation("Account locked status: {IsLocked}", isLocked);

                if (isLocked)
                {
                    _logger.LogWarning("Account is locked due to brute force for email: {Email}", request.Email);
                    return new AuthResult(false,
                        Error: "Account is temporarily locked due to too many failed attempts.",
                        AccountLocked: true);
                }
            }

            // Input sanitization
            _logger.LogInformation("Step 3: Performing input sanitization for email: {Email}", request.Email);
            var isValidEmail = IsValidInput(request.Email);
            var isValidPassword = IsValidInput(request.Password);
            _logger.LogInformation("Email valid: {IsValidEmail}, Password valid: {IsValidPassword}",
                isValidEmail, isValidPassword);

            if (!isValidEmail || !isValidPassword)
            {
                _logger.LogWarning("Invalid input detected for email: {Email} from IP: {IP}",
                    SanitizeForLog(request.Email), ipAddress);

                if (_authSettings.EnableBruteForceProtection)
                {
                    _logger.LogInformation("Recording failed attempt due to invalid input");
                    await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                }

                _logger.LogInformation("Adding delay before returning response");
                await Task.Delay(2000);
                return new AuthResult(false, Error: "Invalid email or password");
            }

            // Find user with roles
            _logger.LogInformation("Step 4: Looking up user in database: {Email}", request.Email);
            var stopwatch = Stopwatch.StartNew();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

            stopwatch.Stop();
            _logger.LogInformation("Database lookup completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            if (user == null)
            {
                _logger.LogWarning("User not found in database: {Email}", request.Email);

                if (_authSettings.EnableBruteForceProtection)
                {
                    _logger.LogInformation("Recording failed attempt for non-existent user");
                    await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                }

                _logger.LogInformation("Adding delay before returning response");
                await Task.Delay(2000);
                return new AuthResult(false, Error: "Invalid email or password");
            }

            _logger.LogInformation("User found - ID: {UserId}, Email: {Email}, IsActive: {IsActive}",
                user.Id, user.Email, user.IsActive);
            _logger.LogInformation("User status - LockedUntil: {LockedUntil}, KycStatus: {KycStatus}, TwoFactorEnabled: {TwoFactorEnabled}",
                user.LockedUntil, user.KycStatus, user.TwoFactorEnabled);
            _logger.LogInformation("User has {RoleCount} roles", user.UserRoles?.Count ?? 0);

            // Check if account is active
            _logger.LogInformation("Step 5: Checking if account is active");
            if (!user.IsActive)
            {
                _logger.LogWarning("Account is deactivated: {Email}", request.Email);
                return new AuthResult(false, Error: "Account is deactivated. Please contact support.");
            }

            // Check if account is locked
            _logger.LogInformation("Step 6: Checking if account is locked");
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                _logger.LogWarning("Account is locked until {LockedUntil} for: {Email}",
                    user.LockedUntil, request.Email);
                return new AuthResult(false,
                    Error: $"Account is locked until {user.LockedUntil:HH:mm}. Please try again later.",
                    AccountLocked: true);
            }

            // KYC access control 
            _logger.LogInformation("Step 7: Checking KYC requirements - RequireVerifiedKyc: {RequireKyc}, User KycStatus: {KycStatus}",
                request.RequireVerifiedKyc, user.KycStatus);

            if (request.RequireVerifiedKyc && user.KycStatus != KycStatus.Verified)
            {
                _logger.LogWarning("KYC verification required but user status is {KycStatus} for: {Email}",
                    user.KycStatus, request.Email);
                return new AuthResult(false,
                    Error: "KYC verification required to access this resource.",
                    NextStep: "complete-kyc");
            }

            // Verify password
            _logger.LogInformation("Step 8: Verifying password for user: {Email}", request.Email);
            stopwatch.Restart();
            var passwordValid = user.VerifyPassword(request.Password);
            stopwatch.Stop();
            _logger.LogInformation("Password verification completed in {ElapsedMs}ms, Result: {PasswordValid}",
                stopwatch.ElapsedMilliseconds, passwordValid);

            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for user: {Email}", request.Email);

                if (_authSettings.EnableBruteForceProtection)
                {
                    _logger.LogInformation("Recording failed password attempt");
                    await _bruteForceProtection.RecordFailedAttemptAsync(request.Email, ipAddress);
                    var failedAttempts = await _bruteForceProtection.GetFailedAttemptsCountAsync(request.Email);
                    _logger.LogInformation("Failed attempts count: {FailedAttempts}", failedAttempts);

                    user.FailedLoginAttempts++;
                    _logger.LogInformation("User failed attempts incremented to: {FailedLoginAttempts}", user.FailedLoginAttempts);

                    // Lock account if max attempts reached
                    if (user.FailedLoginAttempts >= _authSettings.MaxFailedLoginAttempts)
                    {
                        user.LockedUntil = DateTime.UtcNow.AddMinutes(_authSettings.AccountLockoutMinutes);
                        _logger.LogWarning("Account locked for user {Email} until {LockedUntil} due to {Attempts} failed attempts",
                            user.Email, user.LockedUntil, user.FailedLoginAttempts);
                    }

                    _logger.LogInformation("Saving failed attempt state to database");
                    await _context.SaveChangesAsync();

                    return new AuthResult(false,
                        Error: "Invalid email or password",
                        FailedAttempts: failedAttempts,
                        AccountLocked: user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow);
                }

                _logger.LogInformation("Adding delay before returning response");
                await Task.Delay(2000);
                return new AuthResult(false, Error: "Invalid email or password");
            }

            // Check for 2FA if enabled
            _logger.LogInformation("Step 9: Checking 2FA requirements");
            if (user.TwoFactorEnabled)
            {
                _logger.LogInformation("2FA is enabled for user: {Email}", request.Email);

                if (string.IsNullOrEmpty(request.TwoFactorCode))
                {
                    _logger.LogInformation("2FA code required but not provided");
                    return new AuthResult(false,
                        Error: "Two-factor authentication code required",
                        RequiresTwoFactor: true);
                }

                _logger.LogInformation("Validating 2FA code");
                // TODO: Implement actual 2FA validation
                if (!ValidateTwoFactorCode(user, request.TwoFactorCode))
                {
                    _logger.LogWarning("Invalid 2FA code provided for user: {Email}", request.Email);
                    return new AuthResult(false, Error: "Invalid two-factor authentication code");
                }
                _logger.LogInformation("2FA validation successful");
            }

            // Successful login - reset failed attempts
            _logger.LogInformation("Step 10: Login successful, resetting failed attempts for: {Email}", request.Email);
            if (_authSettings.EnableBruteForceProtection)
            {
                _logger.LogInformation("Resetting brute force protection counter");
                await _bruteForceProtection.ResetFailedAttemptsAsync(request.Email);
            }

            // Update user login information
            _logger.LogInformation("Step 11: Updating user login information");
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;
            user.LastLoginUserAgent = userAgent;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;

            // Generate new tokens
            _logger.LogInformation("Step 12: Generating access token for user: {UserId}", user.Id);
            var accessToken = _tokenService.GenerateAccessToken(user);

            _logger.LogInformation("Step 13: Generating refresh token for user: {UserId}", user.Id);
            var refreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;

            // Revoke old tokens (keep last 5)
            _logger.LogInformation("Step 14: Revoking old refresh tokens for user: {UserId}", user.Id);
            await RevokeOldRefreshTokens(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            _logger.LogInformation("New refresh token added to context");

            // Log successful login
            _logger.LogInformation("Step 15: Logging successful login attempt");
            await LogSuccessfulLogin(user.Id, ipAddress, userAgent);

            _logger.LogInformation("Step 16: Saving all changes to database");
            await _context.SaveChangesAsync();
            _logger.LogInformation("Database changes saved successfully");

            _logger.LogInformation("User logged in successfully: {Email} from IP: {IP}",
                SanitizeForLog(user.Email), ipAddress);

            // Determine next steps based on user status
            var nextStep = (user.KycStatus, user.EmailVerified) switch
            {
                (KycStatus.Pending, _) => "complete-kyc",
                (_, false) => "verify-email",
                _ => null
            };

            _logger.LogInformation("Next step for user: {NextStep}", nextStep ?? "dashboard");
            _logger.LogInformation("=== LOGINASYNC COMPLETED SUCCESSFULLY ===");

            return new AuthResult(
                true,
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                RequiresTwoFactor: false,
                NextStep: nextStep
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EXCEPTION in LoginAsync for {Email}: {ErrorMessage}",
                request?.Email ?? "unknown", ex.Message);
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            return new AuthResult(false, Error: "Login failed");
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        try
        {
            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                _logger.LogWarning("Invalid refresh token attempt from IP: {IP}", ipAddress);
                return new AuthResult(false, Error: "Invalid refresh token");
            }

            // Check if user is still active
            if (!token.User.IsActive)
            {
                return new AuthResult(false, Error: "User account is deactivated");
            }

            // Revoke current refresh token
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(token.User);
            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            newRefreshToken.UserId = token.UserId;
            newRefreshToken.ReplacedByToken = newRefreshToken.Token; // Track chain

            _context.RefreshTokens.Add(newRefreshToken);

            // Clean up old tokens
            await RevokeOldRefreshTokens(token.UserId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Token refreshed for user {UserId} from IP: {IP}",
                token.UserId, ipAddress);

            return new AuthResult(
                true,
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken.Token,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token from IP: {IP}", ipAddress);
            return new AuthResult(false, Error: "Token refresh failed");
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        try
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || !token.IsActive)
                return false;

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Token revoked for user {UserId} from IP: {IP}",
                token.UserId, ipAddress);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token from IP: {IP}", ipAddress);
            return false;
        }
    }

    public async Task<AuthResult> LogoutAsync(Guid userId, string? refreshToken = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Revoke specific refresh token
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

                if (token != null && token.IsActive)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = "logout";
                }
            }
            else
            {
                // Revoke all active refresh tokens for user
                var tokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = "logout";
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged out: {UserId}", userId);
            return new AuthResult(true, Message: "Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return new AuthResult(false, Error: "Logout failed");
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
        await _context.SaveChangesAsync();
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
        user.LockedUntil = null;
        await _context.SaveChangesAsync();
        return true;
    }

    #region Helper Methods

    private async Task RevokeOldRefreshTokens(Guid userId, int keepLast = 5)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();

        var tokensToRevoke = activeTokens.Skip(keepLast);

        foreach (var token in tokensToRevoke)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = "cleanup";
        }
    }

    private bool ValidateTwoFactorCode(User user, string code)
    {
        // TODO: Implement actual 2FA validation
        // This would verify against user.TwoFactorSecret
        return true;
    }

    private bool IsValidInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

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

        var sanitized = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d+]", "");

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
                IsSuccess = true,
                DeviceType = GetDeviceType(userAgent),
                Location = await GetLocationFromIp(ipAddress)
            };

            _context.LoginLogs.Add(loginLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log successful login for user {UserId}", userId);
        }
    }

    private string GetDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        userAgent = userAgent.ToLower();

        if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            return "Mobile";

        if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
            return "Tablet";

        return "Desktop";
    }

    private async Task<string?> GetLocationFromIp(string ipAddress)
    {
        // TODO: Implement IP geolocation lookup
        // This could call a service like ip-api.com or MaxMind
        return null;
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

    private DateTime? EnsureUtc(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;

        return dateTime.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc)
            : dateTime.Value.ToUniversalTime();
    }

    #endregion
}