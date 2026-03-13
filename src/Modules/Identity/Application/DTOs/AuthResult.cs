using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs
{
    public record AuthResult
    (
        bool Success,
        string? AccessToken = null,
        string? RefreshToken = null,
        DateTime? ExpiresAt = null,
        string? Error = null,
        int? FailedAttempts = null,
        bool? AccountLocked = null,
        bool? RequiresTwoFactor = null,
        string? NextStep = null,
        string? Message = null
    );
}
