using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public class SSRFProtectionService : ISSRFProtectionService
{
    private readonly HashSet<string> _allowedDomains;
    private readonly ILogger<SSRFProtectionService> _logger;

    // Constructor with IConfiguration parameter
    public SSRFProtectionService(
        IConfiguration configuration,
        ILogger<SSRFProtectionService> logger)
    {
        _logger = logger;

        // Load allowed domains from configuration
        _allowedDomains = LoadAllowedDomains(configuration);
    }

    private HashSet<string> LoadAllowedDomains(IConfiguration configuration)
    {
        try
        {
            // Get the configuration section
            IConfigurationSection section = configuration.GetSection("Security:AllowedDomains");

            // Check if the section exists
            if (!section.Exists())
            {
                _logger.LogWarning("Configuration section 'Security:AllowedDomains' not found. Using default domains.");
                return GetDefaultAllowedDomains();
            }

            
            List<string> domains = section.Get<List<string>>();

            if (domains == null || domains.Count == 0)
            {
                _logger.LogWarning("No domains found in 'Security:AllowedDomains'. Using default domains.");
                return GetDefaultAllowedDomains();
            }

            // Convert to lowercase hashset for case-insensitive comparison
            HashSet<string> result = domains
                .Select(d => d.ToLowerInvariant().Trim())
                .Where(d => !string.IsNullOrEmpty(d))
                .ToHashSet();

            _logger.LogInformation("Loaded {Count} allowed domains from configuration", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading allowed domains from configuration. Using defaults.");
            return GetDefaultAllowedDomains();
        }
    }

    private HashSet<string> GetDefaultAllowedDomains()
    {
        // Default domains for development and common OAuth providers
        return new HashSet<string>
        {
            // OAuth providers
            "accounts.google.com",
            "oauth2.googleapis.com",
            "login.microsoftonline.com",
            "graph.microsoft.com",
            
            // Payment providers
            "api.paystack.com",
            "api.flutterwave.com",
            "secure.authorize.net",
            "api.stripe.com",
            "api.razorpay.com",
            "sandbox.paypal.com",
            "api.paypal.com",
            
            // Development
            "localhost",
            "127.0.0.1",
            
            // Your own domains
            "smartfintechfinancial.com",
            "api.smartfintechfinancial.com"
        };
    }

    // Public method implementations
    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            return false;

        // Check if it's an internal IP address
        if (IsInternalIpAddress(uri.Host))
            return false;

        // Check if domain is allowed
        if (!IsAllowedDomain(uri.Host))
            return false;

        // Additional security checks
        if (IsLocalhost(uri.Host))
            return false;

        if (IsMetadataService(uri.Host))
            return false;

        return true;
    }

    public bool IsInternalIpAddress(string host)
    {
        try
        {
            // Try to resolve host to IP addresses
            IPAddress[] ipAddresses = Dns.GetHostAddresses(host);

            foreach (IPAddress ip in ipAddresses)
            {
                if (IsInternalIp(ip))
                    return true;
            }

            return false;
        }
        catch
        {
            // If we can't resolve, assume it's not internal
            return false;
        }
    }

    public bool IsAllowedDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        string normalizedDomain = domain.ToLowerInvariant();

        // Check exact match or subdomain
        foreach (string allowedDomain in _allowedDomains)
        {
            if (normalizedDomain == allowedDomain ||
                normalizedDomain.EndsWith($".{allowedDomain}"))
            {
                return true;
            }
        }

        return false;
    }

    // Helper methods
    private bool IsInternalIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
            return true;

        byte[] bytes = ip.GetAddressBytes();

        // Check for private IP ranges:
        // 10.0.0.0/8
        if (bytes[0] == 10)
            return true;

        // 172.16.0.0/12
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            return true;

        // 192.168.0.0/16
        if (bytes[0] == 192 && bytes[1] == 168)
            return true;

        // 169.254.0.0/16 (link-local)
        if (bytes[0] == 169 && bytes[1] == 254)
            return true;

        // IPv6 private ranges
        if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal)
            return true;

        return false;
    }

    private bool IsLocalhost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               host.Equals("127.0.0.1") ||
               host.Equals("::1") ||
               host.StartsWith("127.") ||
               host.Equals("[::1]");
    }

    private bool IsMetadataService(string host)
    {
        return host.Equals("169.254.169.254") || // AWS, GCP, Azure metadata
               host.Equals("metadata.google.internal") || // GCP metadata
               host.Contains("metadata"); // Any metadata service
    }

    // Helper to check if a URL is safe to call from your application
    public bool IsSafeToCall(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Additional checks for API calls
        if (url.Contains("://localhost") || url.Contains("://127.0.0.1"))
            return false;

        if (url.Contains("internal") || url.Contains("private") || url.Contains("admin"))
            return false;

        return IsValidUrl(url);
    }
}