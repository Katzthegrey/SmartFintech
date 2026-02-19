using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFintechFinancial.Modules.Identity.Application.Settings;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public class BruteForceProtection : IBruteForceProtection
{
    private readonly IMemoryCache _cache;
    private readonly IdentityDbContext _context;
    private readonly ILogger<BruteForceProtection> _logger;
    private readonly AuthSettings _authSettings;

    public BruteForceProtection(
        IMemoryCache cache,
        IdentityDbContext context,
        ILogger<BruteForceProtection> logger,
        IOptions<AuthSettings> authSettings)
    {
        _cache = cache;
        _context = context;
        _logger = logger;
        _authSettings = authSettings.Value;
    }

    public async Task<bool> IsAccountLockedAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var cacheKey = GetFailedAttemptsCacheKey(normalizedEmail);

        if (_cache.TryGetValue(cacheKey, out int failedAttempts))
        {
            if (failedAttempts >= _authSettings.MaxFailedLoginAttempts)
            {
                _logger.LogWarning("Account locked for email: {Email}", SanitizeForLog(email));
                return true;
            }
        }

        return false;
    }

    public async Task RecordFailedAttemptAsync(string email, string ipAddress, string? userAgent = null)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var cacheKey = GetFailedAttemptsCacheKey(normalizedEmail);

        // Get or create failed attempts count
        var failedAttempts = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_authSettings.AccountLockoutMinutes);
            entry.SlidingExpiration = TimeSpan.FromMinutes(_authSettings.AccountLockoutMinutes);
            return 0;
        }) + 1;

        _cache.Set(cacheKey, failedAttempts, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_authSettings.AccountLockoutMinutes)
        });

        // Log the failed attempt to database for audit trail
        await LogFailedLoginAttempt(normalizedEmail, ipAddress, userAgent ?? "", failedAttempts);

        _logger.LogWarning("Failed login attempt #{Attempt} for email: {Email} from IP: {IP}",
            failedAttempts, SanitizeForLog(email), ipAddress);
    }

    public async Task ResetFailedAttemptsAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var cacheKey = GetFailedAttemptsCacheKey(normalizedEmail);
        _cache.Remove(cacheKey);

        _logger.LogInformation("Reset failed attempts for email: {Email}", SanitizeForLog(email));
    }

    public async Task<int> GetFailedAttemptsCountAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var cacheKey = GetFailedAttemptsCacheKey(normalizedEmail);

        return _cache.TryGetValue(cacheKey, out int failedAttempts) ? failedAttempts : 0;
    }

    //  Using IAuditableEntity pattern
    private async Task LogFailedLoginAttempt(string email, string ipAddress, string userAgent, int attemptNumber)
    {
        try
        {
            var logEntry = new FailedLoginAttempt
            {
                Email = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                AttemptNumber = attemptNumber
            };

            _context.FailedLoginAttempts.Add(logEntry);
            await _context.SaveChangesAsync(); //  This triggers IAuditableEntity auto-population
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log failed login attempt for {Email}", email);
        }
    }

    //  Method to get recent failed attempts using CreatedAt
    public async Task<List<FailedLoginAttempt>> GetRecentFailedAttemptsAsync(string email, TimeSpan timeWindow)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
        
        return await _context.FailedLoginAttempts
            .Where(f => f.Email == email.ToLowerInvariant() && 
                       f.CreatedAt >= cutoffTime)  //  Using CreatedAt from IAuditableEntity
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    //  Method to check if IP is temporarily blocked
    public async Task<bool> IsIpAddressBlockedAsync(string ipAddress, int maxAttempts, TimeSpan timeWindow)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
        
        var attemptCount = await _context.FailedLoginAttempts
            .CountAsync(f => f.IpAddress == ipAddress && 
                            f.CreatedAt >= cutoffTime);  

        return attemptCount >= maxAttempts;
    }

    private string GetFailedAttemptsCacheKey(string email)
    {
        return $"failed_attempts:{email}";
    }

    private string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "[empty]";

        // Remove potentially dangerous characters for logging
        return System.Text.RegularExpressions.Regex.Replace(
            input, @"[^\w\s@\.\-]", "[X]",
            System.Text.RegularExpressions.RegexOptions.None,
            TimeSpan.FromMilliseconds(100));
    }

    public Task RecordFailedAttemptAsync(string email, string ipAddress)
    {
        throw new NotImplementedException();
    }
}