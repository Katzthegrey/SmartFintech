using Microsoft.AspNetCore.Http;

namespace SmartFintechFinancial.API.Extensions;

public static class HttpContextExtensions
{
    public static string GetClientIpAddress(this HttpContext context)
    {
        // Check for forwarded headers (when behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Get the first IP in the chain (client IP)
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for other common proxy headers
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to remote IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    public static string GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers["User-Agent"].ToString() ?? "unknown";
    }

    public static string GetReferrer(this HttpContext context)
    {
        return context.Request.Headers["Referer"].ToString() ?? "direct";
    }
}