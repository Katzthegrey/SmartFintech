using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartFintechFinancial.Modules.Identity.Application.Settings;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;
using SmartGuardFinancial.Modules.Identity.Application.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly IdentityDbContext _context;

    public TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger, IdentityDbContext context)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _context = context;
     }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        var claims = new List<Claim>
        {
            // Standard JWT claims
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            
            // User status claims (matching your User entity)
            new Claim("user_id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("is_active", user.IsActive.ToString()),
            new Claim("email_verified", user.EmailVerified.ToString()),
            new Claim("kyc_status", user.KycStatus.ToString()),
            new Claim("risk_level", user.RiskLevel.ToString()),
            new Claim("subscription_tier", user.SubscriptionTier),
            new Claim("two_factor_enabled", user.TwoFactorEnabled.ToString())
        };

        // Add role claims - CRITICAL for authorization!
        if (user.UserRoles != null && user.UserRoles.Any())
        {
            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null && !string.IsNullOrEmpty(userRole.Role.Name))
                {
                    // Add as standard role claim type
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));

                    // Also add as custom claim for redundancy
                    claims.Add(new Claim("role", userRole.Role.Name));

                    _logger.LogDebug("Added role '{Role}' to token for user {UserId}",
                        userRole.Role.Name, user.Id);
                }
            }
        }
        else
        {
            _logger.LogWarning("No roles found for user {UserId} during token generation", user.Id);
        }

        // Add name claims if available (using FirstName/LastName from your entity)
        if (!string.IsNullOrEmpty(user.FirstName))
        {
            claims.Add(new Claim("first_name", user.FirstName));
            claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName));
        }

        if (!string.IsNullOrEmpty(user.LastName))
        {
            claims.Add(new Claim("last_name", user.LastName));
            claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName));
        }

        // Add full name for convenience (combines FirstName + LastName)
        if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            claims.Add(new Claim("full_name", fullName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, fullName));
        }

        // Add KYC/AML related claims
        claims.Add(new Claim("kyc_status", user.KycStatus.ToString()));
        if (user.KycVerifiedAt.HasValue)
        {
            claims.Add(new Claim("kyc_verified_at", user.KycVerifiedAt.Value.ToString("O")));
        }

        // Add risk management claims
        claims.Add(new Claim("risk_level", user.RiskLevel.ToString()));
        claims.Add(new Claim("is_flagged", user.IsFlaggedForReview.ToString()));

        // Add transaction limits for client-side enforcement
        claims.Add(new Claim("daily_limit", user.DailyTransactionLimit.ToString("F2")));
        claims.Add(new Claim("monthly_limit", user.MonthlyTransactionLimit.ToString("F2")));

        // Add investment preferences
        claims.Add(new Claim("risk_tolerance", user.InvestmentRiskTolerance.ToString()));
        claims.Add(new Claim("investment_goal", user.PrimaryInvestmentGoal.ToString()));

        // Add assigned personnel (useful for authorization decisions)
        if (!string.IsNullOrEmpty(user.AssignedFinancialAdvisor))
        {
            claims.Add(new Claim("assigned_advisor", user.AssignedFinancialAdvisor));
        }

        if (!string.IsNullOrEmpty(user.AssignedComplianceOfficer))
        {
            claims.Add(new Claim("assigned_compliance", user.AssignedComplianceOfficer));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Claims = new Dictionary<string, object>
            {
                // Add token metadata
                ["token_type"] = "access",
                ["token_version"] = "1.0"
            }
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogDebug("Generated access token for user {UserId} with {ClaimCount} claims",
            user.Id, claims.Count);

        return tokenString;
    }

    public RefreshToken GenerateRefreshToken(string ipAddress, Guid? userId = null)
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.Empty, // Use the userId parameter
            Token = Convert.ToBase64String(randomBytes)
                .Replace('+', '-')  // Make URL safe
                .Replace('/', '_')  // Make URL safe
                .Replace("=", ""),  // Remove padding
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            CreatedByIp = ipAddress,
            IsRevoked = false,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "system"
        };

        _logger.LogDebug("Generated refresh token for user {UserId}, expires at {Expiry}",
            refreshToken.UserId, refreshToken.ExpiresAt);

        return refreshToken;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = false, // Important: we want to validate expired tokens
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm for expired token validation");
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating expired token");
            return null;
        }
    }

    public string GenerateEmailConfirmationToken(User user)
    {
        // Create a more secure token using user-specific data
        var tokenData = string.Join("|", new[]
        {
            user.Id.ToString(),
            user.Email,
            "email-confirmation",
            user.PasswordChangedAt?.Ticks.ToString() ?? user.CreatedAt.Ticks.ToString(),
            DateTime.UtcNow.Ticks.ToString()
        });

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenData));
        var token = Convert.ToBase64String(hash)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        _logger.LogDebug("Generated email confirmation token for user {UserId}", user.Id);
        return token;
    }

    public string GeneratePasswordResetToken(User user)
    {
        // Create a secure token tied to the user's current password state
        var tokenData = string.Join("|", new[]
        {
            user.Id.ToString(),
            user.Email,
            "password-reset",
            user.PasswordChangedAt?.Ticks.ToString() ?? "0",
            user.PasswordHash[..20], // Include part of password hash to invalidate after change
            DateTime.UtcNow.Ticks.ToString()
        });

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenData));
        var token = Convert.ToBase64String(hash)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        _logger.LogDebug("Generated password reset token for user {UserId}", user.Id);
        return token;
    }

    public bool ValidateEmailConfirmationToken(string token, User user)
    {
        var expectedToken = GenerateEmailConfirmationToken(user);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(token),
            Encoding.UTF8.GetBytes(expectedToken));
    }

    public bool ValidatePasswordResetToken(string token, User user)
    {
        var expectedToken = GeneratePasswordResetToken(user);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(token),
            Encoding.UTF8.GetBytes(expectedToken));
    }

    public string GenerateEmailConfirmationToken(string email)
    {
        // You'll need to inject DbContext to fetch the user
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning("Attempted to generate email confirmation token for non-existent email: {Email}", email);
            throw new ArgumentException("User not found");
        }

        return GenerateEmailConfirmationToken(user); // Call your existing method
    }

    public string GeneratePasswordResetToken(string email)
    {
        // You'll need to inject DbContext to fetch the user
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            _logger.LogWarning("Attempted to generate password reset token for non-existent email: {Email}", email);
            throw new ArgumentException("User not found");
        }

        return GeneratePasswordResetToken(user); // Call your existing method
    }
}