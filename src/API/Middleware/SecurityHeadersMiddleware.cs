using Microsoft.Extensions.Primitives;

namespace SmartFintechFinancial.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=()");

        // Content Security Policy
        var csp = "default-src 'self'; " +
                 "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                 "style-src 'self' 'unsafe-inline'; " +
                 "img-src 'self' data: https:; " +
                 "font-src 'self'; " +
                 "connect-src 'self'; " +
                 "frame-ancestors 'none'; " +
                 "form-action 'self'; " +
                 "base-uri 'self';";

        context.Response.Headers.Append("Content-Security-Policy", csp);

        // Remove sensitive headers
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

        await _next(context);
    }
}