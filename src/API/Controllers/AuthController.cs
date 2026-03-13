using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SmartFintechFinancial.Modules.Identity.Application.Services;
using SmartFintechFinancial.API.Extensions;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SmartFintechFinancial.Modules.Identity.Application.Validators;
using System.Net;

namespace SmartFintechFinancial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IBruteForceProtection _bruteForceProtection;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IBruteForceProtection bruteForceProtection,
        IRateLimitingService rateLimitingService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _bruteForceProtection = bruteForceProtection;
        _rateLimitingService = rateLimitingService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Get client IP address
            var ipAddress = HttpContext.GetClientIpAddress();

            // Log the incoming request for debugging
            _logger.LogInformation("=== REGISTRATION REASON RECEIVED ===");
            _logger.LogInformation("Email: {Email}", request.Email);
            _logger.LogInformation("FirstName: {FirstName}", request.FirstName);
            _logger.LogInformation("LastName: {LastName}", request.LastName);
            _logger.LogInformation("RegistrationType: {RegistrationType}", request.RegistrationType);
            _logger.LogInformation("AnnualIncome: {AnnualIncome}", request.AnnualIncome);
            _logger.LogInformation("EmploymentStatus: {EmploymentStatus}", request.EmploymentStatus);
            _logger.LogInformation("SourceOfFunds: {SourceOfFunds}", request.SourceOfFunds);
            _logger.LogInformation("InvestmentRiskTolerance: {InvestmentRiskTolerance}", request.InvestmentRiskTolerance);
            _logger.LogInformation("PrimaryInvestmentGoal: {PrimaryInvestmentGoal}", request.PrimaryInvestmentGoal);

            // Validate request with detailed error logging
            var validationResult = await _registerValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                // Log ALL validation errors with full details
                _logger.LogWarning("=== VALIDATION FAILED - {ErrorCount} ERRORS ===", validationResult.Errors.Count);

                foreach (var error in validationResult.Errors)
                {
                    _logger.LogWarning("VALIDATION ERROR - Property: '{Property}', Error: '{Error}', Attempted Value: '{AttemptedValue}', Error Code: '{ErrorCode}'",
                        error.PropertyName,
                        error.ErrorMessage,
                        error.AttemptedValue?.ToString() ?? "NULL",
                        error.ErrorCode);
                }

                _logger.LogWarning("Registration validation failed for email: {Email} from IP: {IP}",
                    SanitizeForLog(request.Email), ipAddress);

                // Return detailed errors in development (optional - remove in production)
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    var errors = validationResult.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage,
                        value = e.AttemptedValue?.ToString()
                    });
                    return BadRequest(new { error = "Invalid registration data", details = errors });
                }

                return BadRequest(new { error = "Invalid registration data" });
            }

            _logger.LogInformation("=== VALIDATION PASSED ===");

            // Check remaining rate limit
            var remainingRequests = await _rateLimitingService.GetRemainingRequestsAsync("register", ipAddress);
            Response.Headers.Add("X-RateLimit-Remaining", remainingRequests.ToString());

            // Process registration
            var result = await _authService.RegisterAsync(request, ipAddress);

            if (!result.Success)
            {
                _logger.LogWarning("Registration service failed for {Email}: {Error}",
                    request.Email, result.Error);
                return BadRequest(new { error = result.Error ?? "Registration Failed" });
            }

            _logger.LogInformation("Registration successful for email: {Email} from IP: {IP}",
                SanitizeForLog(request.Email), ipAddress);

            return Ok(new
            {
                message = result.Message ?? "Registration successful. Please check your email for verification.",
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                tokenType = "Bearer",
                nextStep = result.NextStep,
                requiresEmailVerification = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}",
                SanitizeForLog(request.Email));
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Get client information
            var ipAddress = HttpContext.GetClientIpAddress();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Validate request
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Login validation failed for email: {Email} from IP: {IP}",
                    SanitizeForLog(request.Email), ipAddress);
                return BadRequest(new { error = "Invalid login data" });
            }

            // Check rate limiting
            var remainingRequests = await _rateLimitingService.GetRemainingRequestsAsync("login", ipAddress);
            Response.Headers.Add("X-RateLimit-Remaining", remainingRequests.ToString());

            // Process login
            var result = await _authService.LoginAsync(request, ipAddress, userAgent);

            if (!result.Success)
            {
                if (result.RequiresTwoFactor == true) {
                    return Ok(new
                    {
                        requireTwoFactor = true,
                        message = result.Error ?? "Two-Factor Authentication required"
                    });
                }
                // Include rate limit information if applicable
                var response = new LoginErrorResponse
                {
                    Error = "Invalid email or password"
                };

                if (result.AccountLocked == true)
                {
                    response.Error = "Account temporarily locked due to too many failed attempts";
                    response.Locked = true;
                    response.UnlockAfterMinutes = 15;
                }
                else if (result.FailedAttempts.HasValue)
                {
                    response.FailedAttempts = result.FailedAttempts.Value;
                    Response.Headers.Add("X-Failed-Attempts", result.FailedAttempts.Value.ToString());
                }

                return Unauthorized(response);
            }

            _logger.LogInformation("Login successful for email: {Email} from IP: {IP}",
                SanitizeForLog(request.Email), ipAddress);

            // Successful login - will return JWT token in Day 4
            return Ok(new
            {
                message = "Login successful",
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                tokenType = "Bearer",
                nextStep = result.NextStep,
                requires2FA = false // Will be true when 2FA is enabled
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}",
                SanitizeForLog(request.Email));
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request ) {
        try
        {
            var ipAddress = HttpContext.GetClientIpAddress();
            //Validate request
            if (string.IsNullOrEmpty(request?.RefreshToken))
            {
                return BadRequest(new { error = "Refresh Token is required" });
            }
            //rate limit refresh attempt to prevent abuse
            if (await _rateLimitingService.IsRateLimitedAsync("refresh-token", ipAddress))
            {
                return StatusCode(429, new { error = "too many refresh attempts" });
            }

            await _rateLimitingService.RecordRequestAsync("refresh-token", ipAddress);

            var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            if (!result.Success)
            {
                _logger.LogWarning("Refresh token failed from IP: {IP}", ipAddress);
                return Unauthorized(new { error = result.Error ?? "Invalid refresh token" });
            }
            _logger.LogInformation("Token Refreshed succesfully from IP : {IP}", ipAddress);

            return Ok(new
            {
                accessToken = result.AccessToken,
                refeshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                tokenType = "Bearer"
            });
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { error = "An unexpected error occured" });
        }
    
    }

    //Revoke token endpoint
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request) 
    {
        try
        {
            var ipAddress = HttpContext.GetClientIpAddress();

            if (string.IsNullOrEmpty(request?.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token is required" });
            }

            var revoked = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

            if (!revoked)
            {
                return BadRequest(new { error = "Invalid Token" });
            }

            _logger.LogInformation("Token revoked from IP : {IP}", ipAddress);
            return Ok(new { message = "Token revoked successfully" });
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error revoking token");
            return StatusCode(500, new { error = "An unexpected error occured" });
        }
    }

    //Logout endpoint
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request) 
    {
        try
        {
            var result = await _authService.LogoutAsync(request.UserId, request.RefreshToken);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Error ?? "Logout Failed" });
            }

            _logger.LogInformation("User {UserId} logged out successfully", request.UserId);
            return Ok(new { message = result.Message ?? "Logged out succesfully" });
        }
        catch (Exception ex)
        { 
         _logger.LogError(ex, "Error during logout for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "An unexpected error occured" });
        }
    }


    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            // Rate limit email checking to prevent email enumeration
            var ipAddress = HttpContext.GetClientIpAddress();
            if (await _rateLimitingService.IsRateLimitedAsync("check-email", ipAddress))
            {
                return StatusCode(429, new { error = "Too many requests" });
            }

            await _rateLimitingService.RecordRequestAsync("check-email", ipAddress);

            var exists = await _authService.EmailExistsAsync(email);

            // Return consistent response time to prevent timing attacks
            await Task.Delay(new Random().Next(100, 300));

            return Ok(new
            {
                email = SanitizeForLog(email),
                exists,
                message = exists ? "Email is registered" : "Email is available"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email: {Email}", email);
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpPost("lock-account")]
    public async Task<IActionResult> LockAccount([FromQuery] string email)
    {
        // This would typically require admin privileges
        // For now, we'll just log it
        _logger.LogWarning("Account lock request for email: {Email} from IP: {IP}",
            SanitizeForLog(email), HttpContext.GetClientIpAddress());

        return Ok(new { message = "Account lock request processed" });
    }

    [HttpPost("unlock-account")]
    public async Task<IActionResult> UnlockAccount([FromQuery] string email)
    {
        // This would typically require admin privileges or email verification
        _logger.LogWarning("Account unlock request for email: {Email} from IP: {IP}",
            SanitizeForLog(email), HttpContext.GetClientIpAddress());

        return Ok(new { message = "Account unlock request processed" });
    }

    [HttpGet("security-status")]
    public async Task<IActionResult> GetSecurityStatus()
    {
        var ipAddress = HttpContext.GetClientIpAddress();
        var failedAttempts = await _bruteForceProtection.GetFailedAttemptsCountAsync("test@example.com");
        var remainingRequests = await _rateLimitingService.GetRemainingRequestsAsync("login", ipAddress);

        return Ok(new
        {
            ipAddress,
            userAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
            failedAttempts,
            remainingRequests,
            timestamp = DateTime.UtcNow
        });
    }

    [Authorize]
    [HttpGet("test-auth")]
    public IActionResult TestAuth()
    {
        try
        {
            var userId = User.FindFirst("user_id")?.Value ?? User.FindFirst("sub")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            var firstName = User.FindFirst("first_name")?.Value;
            var lastName = User.FindFirst("last_name")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

            // Get all claims for debugging
            var allClaims = User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            }).ToList();

            _logger.LogInformation("Test auth endpoint accessed by user: {UserId} from IP: {IP}",
                userId, HttpContext.GetClientIpAddress());

            return Ok(new
            {
                Message = "Authentication successful!",
                UserId = userId,
                Email = email,
                Name = $"{firstName} {lastName}".Trim(),
                Roles = roles,
                Permissions = permissions,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                AuthenticationType = User.Identity?.AuthenticationType ?? "Unknown",
                AllClaims = allClaims
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test auth endpoint");
            return StatusCode(500, new { error = "An error occurred processing your request" });
        }
    }

    [Authorize(Roles = "Client")]
    [HttpGet("client-dashboard")]
    public IActionResult ClientDashboard()
    {
        try
        {
            var userId = User.FindFirst("user_id")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = User.FindFirst("first_name")?.Value;
            var lastName = User.FindFirst("last_name")?.Value;
            //ENCODE user-supplied data before returning to client
            var encodedFirstName = WebUtility.HtmlEncode(firstName ?? "");
            var encodedLastName = WebUtility.HtmlEncode(lastName ?? "");
            var encodedFullName = WebUtility.HtmlEncode($"{firstName} {lastName}".Trim());

            _logger.LogInformation("Client dashboard accessed by user: {UserId}", userId);

            return Ok(new
            {
                message = "Welcome to Client Dashboard",
                userId,
                email,
                firstName = encodedFirstName,
                lastName = encodedLastName,
                fullName = encodedFullName,
                features = new[]
                {
                "View Portfolio",
                "Make Deposits",
                "Contact Advisor",
                "View Transaction History"
            },
                limits = new
                {
                    dailyTransfer = 10000.00,
                    monthlyTransfer = 50000.00
                },
                redirectTo = "/dashboard/client",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in client dashboard");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [Authorize(Roles = "Admin, SuperAdmin")]
    [HttpGet("admin-dashboard")]
    public IActionResult AdminDashboard()
    {
        try
        {
            var userId = User.FindFirst("user_id")?.Value;
            var firstName = User.FindFirst("first_name")?.Value;  
            var lastName = User.FindFirst("last_name")?.Value;    

            var encodedFirstName = WebUtility.HtmlEncode(firstName ?? "");
            var encodedLastName = WebUtility.HtmlEncode(lastName ?? "");

            _logger.LogInformation("Admin dashboard accessed by user: {UserId}", userId);

            return Ok(new
            {
                message = "Welcome to Admin Dashboard",
                userId,
                firstName = encodedFirstName,
                lastName = encodedLastName,
                features = new[]
                {
                "User Management",
                "Compliance Review",
                "System Settings",
                "Risk Monitoring",
                "Audit Logs"
            },
                redirectTo = "/dashboard/admin",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in admin dashboard");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
    [Authorize(Roles = "Investor")]
    [HttpGet("investor-dashboard")]
    public IActionResult InvestorDashboard()
    {
        try
        {
            var userId = User.FindFirst("user_id")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = User.FindFirst("first_name")?.Value;
            var lastName = User.FindFirst("last_name")?.Value;

            var encodedFirstName = WebUtility.HtmlEncode(firstName ?? "");
            var encodedLastName = WebUtility.HtmlEncode(lastName ?? "");

            _logger.LogInformation("Investor dashboard accessed by user: {UserId}", userId);

            return Ok(new
            {
                message = "Welcome to Investor Dashboard",
                userId,
                email,
                firstName = encodedFirstName,
                lastName = encodedLastName,
                features = new[]
                {
                "Advanced Analytics",
                "Trading Platform",
                "Portfolio Optimization",
                "Market Insights"
            },
                limits = new
                {
                    dailyTransfer = 50000.00,
                    monthlyTransfer = 250000.00
                },
                redirectTo = "/dashboard/investor",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in investor dashboard");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [Authorize(Roles = "PremiumInvestor")]
    [HttpGet("premium-dashboard")]
    public IActionResult PremiumDashboard()
    {
        try
        {
            var userId = User.FindFirst("user_id")?.Value;
            var firstName = User.FindFirst("first_name")?.Value;
            var lastName = User.FindFirst("last_name")?.Value;

            var encodedFirstName = WebUtility.HtmlEncode(firstName ?? "");
            var encodedLastName = WebUtility.HtmlEncode(lastName ?? "");

            return Ok(new
            {
                message = "Welcome to Premium Investor Dashboard",
                userId,
                firstName = encodedFirstName,
                lastName = encodedLastName,
                features = new[]
                {
                "Wealth Management",
                "Private Equity Access",
                "Hedge Fund Investments",
                "Dedicated Advisor",
                "Tax Optimization"
            },
                limits = new
                {
                    dailyTransfer = 100000.00,
                    monthlyTransfer = 500000.00
                },
                redirectTo = "/dashboard/premium",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in premium dashboard");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [Authorize(Roles = "BusinessInvestor")]
    [HttpGet("business-dashboard")]
    public IActionResult BusinessDashboard()
    {
        try
        {
            var userId = User.FindFirst("user_id")?.Value;
            var firstName = User.FindFirst("first_name")?.Value;
            var lastName = User.FindFirst("last_name")?.Value;

            var encodedFirstName = WebUtility.HtmlEncode(firstName ?? "");
            var encodedLastName = WebUtility.HtmlEncode(lastName ?? "");

            return Ok(new
            {
                message = "Welcome to Business Investor Dashboard",
                userId,
                firstName = encodedFirstName,
                lastName = encodedLastName,
                features = new[]
                {
                "Corporate Account Management",
                "Team Access Controls",
                "Bulk Transactions",
                "API Access",
                "Compliance Reporting"
            },
                limits = new
                {
                    dailyTransfer = 500000.00,
                    monthlyTransfer = 2000000.00
                },
                redirectTo = "/dashboard/business",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in business dashboard");
            return StatusCode(500, new { error = "An error occurred" });
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
}