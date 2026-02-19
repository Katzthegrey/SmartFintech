using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFintechFinancial.Modules.Identity.Application.Settings;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public class RateLimitingService : IRateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingService> _logger;
    private readonly AuthSettings _authSettings;

    public RateLimitingService(
        IMemoryCache cache,
        ILogger<RateLimitingService> logger,
        IOptions<AuthSettings> authSettings)
    {
        _cache = cache;
        _logger = logger;
        _authSettings = authSettings.Value;
    }

    public async Task<bool> IsRateLimitedAsync(string endpoint, string identifier)
    {
        if (!_authSettings.EnableRateLimiting)
            return false;

        var cacheKey = GetRateLimitCacheKey(endpoint, identifier);

        if (_cache.TryGetValue(cacheKey, out RateLimitInfo rateLimitInfo))
        {
            if (rateLimitInfo.RequestCount >= _authSettings.RateLimitRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded for {Endpoint} by {Identifier}",
                    endpoint, SanitizeIdentifier(identifier));
                return true;
            }
        }

        return false;
    }

    public async Task RecordRequestAsync(string endpoint, string identifier)
    {
        if (!_authSettings.EnableRateLimiting)
            return;

        var cacheKey = GetRateLimitCacheKey(endpoint, identifier);
        var now = DateTime.UtcNow;

        var rateLimitInfo = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new RateLimitInfo
            {
                FirstRequest = now,
                RequestCount = 0
            };
        });

        rateLimitInfo.RequestCount++;
        rateLimitInfo.LastRequest = now;

        _cache.Set(cacheKey, rateLimitInfo, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });
    }

    public async Task<int> GetRemainingRequestsAsync(string endpoint, string identifier)
    {
        if (!_authSettings.EnableRateLimiting)
            return int.MaxValue;

        var cacheKey = GetRateLimitCacheKey(endpoint, identifier);

        if (_cache.TryGetValue(cacheKey, out RateLimitInfo rateLimitInfo))
        {
            return Math.Max(0, _authSettings.RateLimitRequestsPerMinute - rateLimitInfo.RequestCount);
        }

        return _authSettings.RateLimitRequestsPerMinute;
    }

    private string GetRateLimitCacheKey(string endpoint, string identifier)
    {
        return $"rate_limit:{endpoint}:{identifier}";
    }

    private string SanitizeIdentifier(string identifier)
    {
        // For IP addresses, mask the last octet
        if (System.Net.IPAddress.TryParse(identifier, out var ipAddress))
        {
            var parts = identifier.Split('.');
            if (parts.Length == 4)
            {
                return $"{parts[0]}.{parts[1]}.{parts[2]}.[X]";
            }
        }

        return identifier.Length > 20 ? identifier.Substring(0, 20) + "..." : identifier;
    }

    private class RateLimitInfo
    {
        public DateTime FirstRequest { get; set; }
        public DateTime LastRequest { get; set; }
        public int RequestCount { get; set; }
    }
}