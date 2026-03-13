using System;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs;

public record RegistrationResponse(
    bool Success,
    string? Message = null,
    Guid? UserId = null,
    string? Email = null,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    bool RequiresEmailVerification = true,
    bool RequiresKycVerification = false,
    string? NextStep = null // "verify-email", "complete-kyc", "wait-for-approval", etc.
);