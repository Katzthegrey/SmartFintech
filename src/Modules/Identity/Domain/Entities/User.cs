using System;
using System.Collections.Generic;
using SmartFintechFinancial.Shared.Infrastructure.Persistence;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities;

public class User : IAuditableEntity
{
    // Basic fields
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;

    // === SECURITY ENHANCEMENTS ===

    // Account security
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public DateTime? PasswordExpiresAt { get; set; }

    // 2FA
    public string? TwoFactorSecret { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? BackupCodes { get; set; } 

    // Audit & tracking
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public string? LastLoginUserAgent { get; set; }

    // KYC/AML Compliance (CRITICAL for fintech)
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public DateTime? KycVerifiedAt { get; set; }

    // Portfolio app preferences
    public string? Timezone { get; set; }
    public string Currency { get; set; } = "R";
    public string Language { get; set; } = "en";
    public string SubscriptionTier { get; set; } = "Free";

    // GDPR/Consent
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentGivenAt { get; set; }
    public bool MarketingOptIn { get; set; }

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    // === IAuditableEntity implementation ===
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // === DOMAIN METHODS ===

    // Password methods
    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12); // Increased security

    public bool VerifyPassword(string password)
        => BCrypt.Net.BCrypt.Verify(password, PasswordHash);

    // Security status methods
    public bool IsLockedOut => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    public bool IsPasswordExpired => PasswordExpiresAt.HasValue && PasswordExpiresAt.Value < DateTime.UtcNow;

    public bool RequiresPasswordChange => IsPasswordExpired ||
                                         (PasswordChangedAt.HasValue &&
                                          (DateTime.UtcNow - PasswordChangedAt.Value).TotalDays > 90);

    // Login security methods
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
    }

    public void ResetFailedLogins()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public bool CanAuthenticate()
    {
        return IsActive &&
               !IsLockedOut &&
               EmailVerified &&
               (!PasswordExpiresAt.HasValue || !IsPasswordExpired);
    }

    // 2FA methods
    public bool HasValidBackupCode(string code)
    {
        if (string.IsNullOrEmpty(BackupCodes)) return false;
        // Simple check - in reality, parse JSON and validate
        return BackupCodes.Contains(code);
    }

    public void UseBackupCode(string code)
    {
        // Implementation: Remove used code from BackupCodes JSON
    }
}

// === SUPPORTING ENUM ===
public enum KycStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
    UnderReview = 3
}