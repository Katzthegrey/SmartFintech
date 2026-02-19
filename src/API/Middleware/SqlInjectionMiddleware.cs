using System.Text.RegularExpressions;

namespace SmartGuardFinancial.API.Middleware;

public class SqlInjectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SqlInjectionMiddleware> _logger;
    private readonly HashSet<string> _excludedPaths = new()
    {
        "/health",
        "/metrics",
        "/favicon.ico"
    };

    public SqlInjectionMiddleware(RequestDelegate next, ILogger<SqlInjectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip middleware for excluded paths
        if (_excludedPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        // Check query string
        if (context.Request.QueryString.HasValue)
        {
            var queryString = context.Request.QueryString.Value;
            if (ContainsSqlInjection(queryString))
            {
                await BlockRequest(context, "SQL injection detected in query string");
                return;
            }
        }

        // Check form data
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();

            foreach (var key in form.Keys)
            {
                var values = form[key];
                foreach (var value in values)
                {
                    if (ContainsSqlInjection(value) || ContainsSqlInjection(key))
                    {
                        await BlockRequest(context, $"SQL injection detected in form field: {key}");
                        return;
                    }
                }
            }
        }

        // Check JSON body for POST/PUT/PATCH
        if (context.Request.Method == "POST" ||
            context.Request.Method == "PUT" ||
            context.Request.Method == "PATCH")
        {
            // Store original body stream
            var originalBodyStream = context.Request.Body;

            try
            {
                // Read the request body
                using var memoryStream = new MemoryStream();
                await originalBodyStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var requestBody = new StreamReader(memoryStream).ReadToEnd();

                // Check for SQL injection in body
                if (ContainsSqlInjection(requestBody))
                {
                    await BlockRequest(context, "SQL injection detected in request body");
                    return;
                }

                // Reset the request body stream
                memoryStream.Position = 0;
                context.Request.Body = memoryStream;

                await _next(context);

                // Restore original stream
                context.Request.Body = originalBodyStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking request body for SQL injection");
                context.Request.Body = originalBodyStream;
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private bool ContainsSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Comprehensive SQL injection patterns
        var patterns = new[]
        {
            // SQL keywords in suspicious contexts
            @"\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|EXEC|EXECUTE|ALTER|CREATE|TRUNCATE|MERGE)\b\s+[\w\*]",
            
            // SQL comments and statement terminators
            @"(\-\-|\#|\/\*|\*\/|;|\')",
            
            // Always true conditions
            @"(\b(OR|AND)\b\s+\d+\s*=\s*\d+)",
            @"(\b(OR|AND)\b\s+['""]\s*=\s*['""])",
            @"(\b(OR|AND)\b\s+['""]\w+['""]\s*=\s*['""]\w+['""])",
            
            // Time-based delays
            @"(WAITFOR\s+DELAY\s+['""]\d+:\d+:\d+['""])",
            @"(SLEEP\s*\(\s*\d+\s*\))",
            @"(BENCHMARK\s*\(\s*\d+)",
            
            // Error-based injection
            @"(CONVERT\s*\(|CAST\s*\()",
            @"(EXTRACTVALUE\s*\(|UPDATEXML\s*\()",
            
            // System tables and functions
            @"(\b(SYSOBJECTS|SYSUSERS|SYSCOLUMNS|SYSTABLES|INFORMATION_SCHEMA)\b)",
            @"(\b(XP_|SP_|FN_|MSys)\w*\b)",
            
            // File system access
            @"(LOAD_FILE\s*\()",
            @"(INTO\s+OUTFILE\s+['""])",
            @"(INTO\s+DUMPFILE\s+['""])",
            
            // Command execution
            @"(\b(CMDEXEC|XP_CMDSHELL|REGREAD)\b)",
            
            // Union-based injection patterns
            @"(UNION\s+ALL\s+SELECT)",
            @"(UNION\s+SELECT\s+\d+)",
            
            // Blind injection patterns
            @"(IF\s*\(\s*\d+\s*=\s*\d+\s*\))",
            @"(CASE\s+WHEN\s+\d+\s*=\s*\d+\s*THEN)",
            
            // Hexadecimal encoding
            @"(0x[0-9A-F]+)",
            
            // Concatenation for obfuscation
            @"(CHAR\s*\(\s*\d+\s*\)\s*\+\s*CHAR\s*\(\s*\d+\s*\))",
            
            // Comment obfuscation
            @"(\/\*\w+\*\/)",
            
            // Bypass techniques
            @"(\b(OR|AND)\b\s+['""]\s*\|\|)",
            @"(\b(OR|AND)\b\s+\d+\s*\|\|)",
        };

        foreach (var pattern in patterns)
        {
            try
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled,
                    TimeSpan.FromMilliseconds(100)))
                {
                    return true;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                // If regex times out, treat as suspicious
                _logger.LogWarning("Regex timeout while checking pattern: {Pattern}", pattern);
                return true;
            }
        }

        return false;
    }

    private async Task BlockRequest(HttpContext context, string reason)
    {
        _logger.LogWarning("Blocked request from {IP} to {Path}: {Reason}",
            context.Connection.RemoteIpAddress,
            context.Request.Path,
            reason);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Invalid request",
            requestId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}