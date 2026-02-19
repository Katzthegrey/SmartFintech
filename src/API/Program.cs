using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder; // For WebApplication
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // For Configuration
using Microsoft.Extensions.DependencyInjection; // For DI
using Microsoft.Extensions.Diagnostics.HealthChecks; // For health checks
using Microsoft.Extensions.Hosting; // For IHostEnvironment
using Microsoft.Extensions.Logging; // For logging
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFintechFinancial.Modules.Identity.Application.DTOs; // For validators
using SmartFintechFinancial.Modules.Identity.Application.Services;
using SmartFintechFinancial.Modules.Identity.Application.Settings;
using SmartFintechFinancial.Modules.Identity.Application.Validators;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;
using System.Reflection; // For Assembly
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// ========== LOCAL VARIABLES FOR REUSED SERVICES ==========
var environment = builder.Environment;
var configuration = builder.Configuration;

// ========== DATABASE CONFIGURATION ==========
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(60);
        npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).GetTypeInfo().Assembly.FullName); // Fixed
        npgsqlOptions.SetPostgresVersion(new Version(14, 0));
    });

    if (environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

// ========== APPLICATION SETTINGS ==========
builder.Services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"));
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

// ========== SECURITY SERVICES ==========
// Memory cache for rate limiting and brute force protection
builder.Services.AddMemoryCache();

// Get JWT settings for token service
var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
{
    throw new InvalidOperationException("JWT settings not configured properly.");
}

// Authentication & Authorization services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBruteForceProtection, BruteForceProtection>(); // Fixed: Removed "Service" suffix
builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
builder.Services.AddScoped<ISSRFProtectionService, SSRFProtectionService>();
//builder.Services.AddScoped<ITokenService, TokenService>(); // Make sure this exists

// ========== JWT AUTHENTICATION ==========
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        RequireSignedTokens = true
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed: {Exception}", context.Exception.Message);

            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            logger.LogInformation("Token validated for user ID: {UserId}", userId);
            await Task.CompletedTask;
        }
    };
});

// ========== AUTHORIZATION ==========
builder.Services.AddAuthorization(options =>
{
    // Default policy: require authenticated user
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Role-based policies (for Day 4/5)
    options.AddPolicy("RequireCustomerRole", policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin", "SuperAdmin", "FinanceAdmin"));

    options.AddPolicy("RequireStaffRole", policy =>
        policy.RequireRole("Support", "Analyst", "AccountManager"));
});

// ========== VALIDATION SERVICES ==========
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// ========== CONTROLLERS & API ==========
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = environment.IsDevelopment();
    });

// ========== CORS CONFIGURATION ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        var allowedOrigins = configuration.GetSection("Security:CorsOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "https://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("X-RateLimit-Remaining", "X-Failed-Attempts", "Token-Expired")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// ========== SWAGGER/OPENAPI ==========
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartFintech Financial API",
        Version = "v1",
        Description = "Financial services and authentication API",
        Contact = new OpenApiContact
        {
            Name = "SmartFintech Team",
            Email = "support@smartfintechfinancial.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ========== HEALTH CHECKS ==========
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>()
    .AddCheck<AuthServiceHealthCheck>("auth_service");

// ========== HTTP CONTEXT ACCESSOR ==========
builder.Services.AddHttpContextAccessor();

// ========== LOGGING ==========
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddConfiguration(configuration.GetSection("Logging"));

    if (environment.IsDevelopment())
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
    else
    {
        logging.SetMinimumLevel(LogLevel.Information);
    }
});

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========

// Exception handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFintech API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableTryItOutByDefault();
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});

// Apply migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Database migrations applied successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error applying database migrations");
    // Don't crash in development
    if (!app.Environment.IsDevelopment())
    {
        throw;
    }
}

app.UseHttpsRedirection();
app.UseRouting();

// CORS must come after Routing and before Authentication
app.UseCors("ReactApp");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();

// Health check endpoint (no auth required)
app.MapHealthChecks("/health").AllowAnonymous();

// Error handling endpoint
app.Map("/error", () => Results.Problem("An error occurred", statusCode: 500));

// Welcome endpoint
app.MapGet("/", () => Results.Json(new
{
    message = "SmartFintech Financial API",
    version = "1.0.0",
    status = "operational",
    documentation = "/swagger",
    health = "/health"
})).AllowAnonymous();

try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting SmartFintech Financial API...");
    logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    logger.LogInformation("Database: PostgreSQL");

    await app.RunAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Application terminated unexpectedly");
    throw;
}

// ========== HEALTH CHECK CLASS ==========
public class AuthServiceHealthCheck : IHealthCheck
{
    private readonly IAuthService _authService;

    public AuthServiceHealthCheck(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple check - try to see if auth service is responsive
            var exists = await _authService.EmailExistsAsync("test@example.com");
            return HealthCheckResult.Healthy("Auth service is operational");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Auth service check failed", ex);
        }
    }
}