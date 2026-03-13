using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using SmartFintechFinancial.Modules.Identity.Application.Services;
using SmartFintechFinancial.Modules.Identity.Application.Settings;
using SmartFintechFinancial.Modules.Identity.Application.Validators;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;
using SmartGuardFinancial.Modules.Identity.Application.Services;
using System.Diagnostics;
using System.Reflection;
using System.Text;

// ========== NLOG INITIALIZATION ==========
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
try
{
    if (!Directory.Exists(logDirectory))
    {
        Directory.CreateDirectory(logDirectory);
    }
}
catch
{
    // Fallback to temp directory if can't write to current directory
    logDirectory = Path.Combine(Path.GetTempPath(), "SmartFintechLogs");
    Directory.CreateDirectory(logDirectory);
}

var builder = WebApplication.CreateBuilder(args);

// Add NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// ========== SERVICES ==========
var environment = builder.Environment;
var configuration = builder.Configuration;

// Database
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"=== CONNECTION STRING: {connectionString} ===");
    Debug.WriteLine($"=== CONNECTION STRING: {connectionString} ===");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
        npgsqlOptions.CommandTimeout(60);
        npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).GetTypeInfo().Assembly.FullName);
    });

    if (environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Settings
builder.Services.Configure<AuthSettings>(configuration.GetSection("AuthSettings"));
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

// Security services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBruteForceProtection, BruteForceProtection>();
builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
builder.Services.AddScoped<ISSRFProtectionService, SSRFProtectionService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Authentication
var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings?.Key == null)
    throw new InvalidOperationException("JWT settings not configured properly.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin", "SuperAdmin", "FinanceAdmin"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Support", "Analyst", "AccountManager"));
});

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartFintech Financial API",
        Version = "v1"
    });

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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>()
    .AddCheck<AuthServiceHealthCheck>("auth_service");

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ========== MIDDLEWARE ==========
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFintech API v1"));
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

// Apply migrations
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await dbContext.Database.MigrateAsync();
    app.Logger.LogInformation("Database migrations applied successfully.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error applying database migrations");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

// Simple welcome endpoint
app.MapGet("/", () => Results.Json(new
{
    message = "SmartFintech Financial API",
    version = "1.0.0",
    status = "operational",
    documentation = "/swagger"
})).AllowAnonymous();

try
{
    app.Logger.LogInformation("Starting SmartFintech Financial API...");
    await app.RunAsync();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

// Health check class
public class AuthServiceHealthCheck : IHealthCheck
{
    private readonly IAuthService _authService;
    public AuthServiceHealthCheck(IAuthService authService) => _authService = authService;
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await _authService.EmailExistsAsync("test@example.com");
        return HealthCheckResult.Healthy("Auth service is operational");
    }
}