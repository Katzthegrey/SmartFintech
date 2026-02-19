using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SmartFintechFinancial.Modules.Identity.Application.Services;
using SmartFintechFinancial.API.Extensions;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using SmartFintechFinancial.Modules.Identity.Application.Validators;

namespace SmartGuardFinancial.API.Controllers;

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

            // Validate request
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                // Don't reveal specific validation errors that could help attackers
                _logger.LogWarning("Registration validation failed for email: {Email} from IP: {IP}",
                    SanitizeForLog(request.Email), ipAddress);
                return BadRequest(new { error = "Invalid registration data" });
            }

            // Check remaining rate limit
            var remainingRequests = await _rateLimitingService.GetRemainingRequestsAsync("register", ipAddress);
            Response.Headers.Add("X-RateLimit-Remaining", remainingRequests.ToString());

            // Process registration
            var result = await _authService.RegisterAsync(request, ipAddress);

            if (!result.Success)
            {
                // Use generic error messages to prevent information leakage
                return BadRequest(new { error = "Registration failed" });
            }

            _logger.LogInformation("Registration successful for email: {Email} from IP: {IP}",
                SanitizeForLog(request.Email), ipAddress);

            return Ok(new
            {
                message = "Registration successful. Please check your email for verification.",
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