using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string ipAddress);
    Task<AuthResult> LoginAsync(LoginRequest request, string ipAddress, string userAgent);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> VerifyPasswordAsync(string email, string password);
    Task<bool> LockAccountAsync(string email);
    Task<bool> UnlockAccountAsync(string email);
}

public record AuthResult(
    bool Success,
    string? Error = null,
    int? FailedAttempts = null,
    bool? AccountLocked = null);