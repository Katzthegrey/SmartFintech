using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using System.Security.Claims;

namespace SmartGuardFinancial.Modules.Identity.Application.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(string ipAddress, Guid? userId = null);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    string GenerateEmailConfirmationToken(string email);
    string GeneratePasswordResetToken(string email);
}