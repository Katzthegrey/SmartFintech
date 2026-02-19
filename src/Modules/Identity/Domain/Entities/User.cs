using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SmartFintechFinancial.Modules.Identity.Domain.Constants;
using SmartFintechFinancial.Shared.Infrastructure.Persistence;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(KycStatus))]
[Index(nameof(RiskLevel))]
[Index(nameof(SubscriptionTier))]


public class User : IAuditableEntity
{
    // ===== BASIC FIELDS =====
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;

    // ===== SECURITY ENHANCEMENTS =====

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

    // ===== KYC/AML COMPLIANCE (CRITICAL for fintech) =====

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    // Financial Profile (for risk assessment)
    public decimal? AnnualIncome { get; set; }

    [MaxLength(50)]
    public string? EmploymentStatus { get; set; }

    [MaxLength(50)]
    public string? SourceOfFunds { get; set; }

    // Tax Identification (encrypted)
    [MaxLength(100)]
    public string? TaxIdNumber { get; set; }

    public KycStatus KycStatus { get; set; } = KycStatus.Pending;
    public DateTime? KycVerifiedAt { get; set; }

    [MaxLength(100)]
    public string? KycVerifiedBy { get; set; }
    public string? KycRejectionReason { get; set; }

    // ===== ROLE-BASED AUTHORIZATION (NEW) =====

    // Navigation property for roles
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Helper property to get role names
    [NotMapped]
    public ICollection<string> RoleNames => UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();

    // ===== FRAUD DETECTION & RISK MANAGEMENT (NEW) =====

    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    public DateTime? RiskAssessedAt { get; set; }

    [MaxLength(100)]
    public string? RiskAssessedBy { get; set; }
    public string? RiskNotes { get; set; }

    // Transaction limits based on risk level
    public decimal DailyTransactionLimit { get; set; } = 10000.00m;
    public decimal MonthlyTransactionLimit { get; set; } = 50000.00m;

    // Flag for suspicious activity
    public bool IsFlaggedForReview { get; set; } = false;
    public DateTime? FlaggedAt { get; set; }
    public string? FlagReason { get; set; }

    // ===== PORTFOLIO APP PREFERENCES =====

    [MaxLength(50)]
    public string? Timezone { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "R";

    [MaxLength(5)]
    public string Language { get; set; } = "en";

    public string SubscriptionTier { get; set; } = "Free";

    // Investment preferences
    public RiskTolerance InvestmentRiskTolerance { get; set; } = RiskTolerance.Moderate;
    public InvestmentGoal PrimaryInvestmentGoal { get; set; } = InvestmentGoal.Growth;

    [MaxLength(50)]
    public string? PreferredInvestmentTypes { get; set; } // JSON array of preferred types

    // ===== GDPR/CONSENT =====
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentGivenAt { get; set; }
    public bool MarketingOptIn { get; set; }

    [MaxLength(1000)]
    public string? ConsentPreferences { get; set; } // JSON for granular consent

    // ===== SUPPORT & COMPLIANCE =====

    [MaxLength(50)]
    public string? AssignedFinancialAdvisor { get; set; } // ID of assigned advisor

    [MaxLength(50)]
    public string? AssignedComplianceOfficer { get; set; } // ID of compliance officer

    public bool RequiresPeriodicReview { get; set; } = false;
    public DateTime? NextReviewDate { get; set; }

    // ===== NAVIGATION PROPERTIES =====
   
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    // Login logs for audit trail
    public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();

    // Failed login attempts
    public virtual ICollection<FailedLoginAttempt> FailedLoginAttemptsLog { get; set; } = new List<FailedLoginAttempt>();

    // ===== IAuditableEntity implementation =====
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ===== DOMAIN METHODS =====

    // Password methods
    public static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool VerifyPassword(string password)
    {
        // Add null check for PasswordHash
        if (string.IsNullOrEmpty(PasswordHash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    // Security status methods
    public bool IsLockedOut => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    public bool IsPasswordExpired => PasswordExpiresAt.HasValue && PasswordExpiresAt.Value < DateTime.UtcNow;

    public bool RequiresPasswordChange => IsPasswordExpired ||
                                         (PasswordChangedAt.HasValue &&
                                          (DateTime.UtcNow - PasswordChangedAt.Value).TotalDays > 90);

    // ===== ROLE-BASED METHODS (NEW) =====

    public bool HasRole(string roleName)
        => UserRoles?.Any(ur => ur.Role.Name == roleName) ?? false;

    public bool HasAnyRole(params string[] roleNames)
        => UserRoles?.Any(ur => roleNames.Contains(ur.Role.Name)) ?? false;

    public bool HasAllRoles(params string[] roleNames)
        => UserRoles?.Count(ur => roleNames.Contains(ur.Role.Name)) == roleNames.Length;

    public string GetPrimaryRole()
    {
        if (UserRoles == null || !UserRoles.Any())
            return RoleConstants.Client;

        var rolePriority = new Dictionary<string, int>
        {
            { RoleConstants.SuperAdmin, 100 },
            { RoleConstants.ComplianceOfficer, 90 },
            { RoleConstants.FinanceAdmin, 80 },
            { RoleConstants.FraudAnalyst, 75 },
            { RoleConstants.WealthManager, 70 },
            { RoleConstants.FinancialAdvisor, 60 },
            { RoleConstants.SupportAgent, 50 },
            { RoleConstants.BusinessInvestor, 40 },
            { RoleConstants.PremiumInvestor, 30 },
            { RoleConstants.Investor, 20 },
            { RoleConstants.Client, 10 }
        };

        return UserRoles
            .Select(ur => ur.Role.Name)
            .OrderByDescending(r => rolePriority.GetValueOrDefault(r, 0))
            .First();
    }

    public bool IsInRoleCategory(string category)
    {
        var roleCategories = new Dictionary<string, string>
        {
            { RoleConstants.Client, "Client" },
            { RoleConstants.Investor, "Client" },
            { RoleConstants.PremiumInvestor, "Client" },
            { RoleConstants.BusinessInvestor, "Client" },
            { RoleConstants.FinancialAdvisor, "Advisor" },
            { RoleConstants.WealthManager, "Management" },
            { RoleConstants.SupportAgent, "Support" },
            { RoleConstants.FraudAnalyst, "Security" },
            { RoleConstants.ComplianceOfficer, "Compliance" },
            { RoleConstants.FinanceAdmin, "Admin" },
            { RoleConstants.SuperAdmin, "Admin" }
        };

        return UserRoles?.Any(ur =>
            roleCategories.TryGetValue(ur.Role.Name, out var roleCat) &&
            roleCat == category) ?? false;
    }

    // ===== FRAUD & RISK METHODS (NEW) =====

    public void FlagForReview(string reason, string flaggedBy)
    {
        IsFlaggedForReview = true;
        FlaggedAt = DateTime.UtcNow;
        FlagReason = reason;

        // Increase risk level when flagged
        if (RiskLevel != RiskLevel.High)
            RiskLevel = RiskLevel.Medium;

        // Notify compliance if high risk
        if (RiskLevel == RiskLevel.High)
        {
            // This would trigger a notification in real implementation
        }
    }

    public void ClearFlag(string clearedBy)
    {
        IsFlaggedForReview = false;
        FlagReason = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = clearedBy;
    }

    public void UpdateRiskLevel(RiskLevel newLevel, string assessedBy, string notes = null)
    {
        RiskLevel = newLevel;
        RiskAssessedAt = DateTime.UtcNow;
        RiskAssessedBy = assessedBy;
        RiskNotes = notes;

        // Update transaction limits based on risk
        switch (newLevel)
        {
            case RiskLevel.Low:
                DailyTransactionLimit = 50000.00m;
                MonthlyTransactionLimit = 250000.00m;
                break;
            case RiskLevel.Medium:
                DailyTransactionLimit = 10000.00m;
                MonthlyTransactionLimit = 50000.00m;
                break;
            case RiskLevel.High:
                DailyTransactionLimit = 1000.00m;
                MonthlyTransactionLimit = 5000.00m;
                break;
            case RiskLevel.Restricted:
                DailyTransactionLimit = 0.00m;
                MonthlyTransactionLimit = 0.00m;
                break;
        }
    }

    public bool CanTransact(decimal amount)
    {
        if (RiskLevel == RiskLevel.Restricted)
            return false;

        if (IsFlaggedForReview)
            return false;

        if (amount > DailyTransactionLimit)
            return false;

        // Additional checks based on role
        if (HasRole(RoleConstants.Client) && amount > 10000.00m)
            return false;

        // Allow higher limits for Investor/PremiumInvestor/BusinessInvestor
        if (HasAnyRole(RoleConstants.Investor, RoleConstants.PremiumInvestor, RoleConstants.BusinessInvestor))
        {
            // Investors can transact up to their daily limit (50000 for Low risk)
            return amount <= DailyTransactionLimit;
        }

        return true;
    }

    // ===== LOGIN SECURITY METHODS =====

    public void RecordFailedLogin(string ipAddress = null)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
            UpdateRiskLevel(RiskLevel.Medium, "system", "Multiple failed login attempts");
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
               (!PasswordExpiresAt.HasValue || !IsPasswordExpired) &&
               KycStatus == KycStatus.Verified &&
               RiskLevel != RiskLevel.Restricted;
    }


    // ===== KYC METHODS =====

    public void ApproveKyc(string verifiedBy)
    {
        KycStatus = KycStatus.Verified;
        KycVerifiedAt = DateTime.UtcNow;
        KycVerifiedBy = verifiedBy;
        KycRejectionReason = null;
    }

    public void RejectKyc(string reason, string rejectedBy)
    {
        KycStatus = KycStatus.Rejected;
        KycRejectionReason = reason;
        UpdateRiskLevel(RiskLevel.High, rejectedBy, $"KYC rejected: {reason}");
    }

    // ===== 2FA METHODS =====

    public bool HasValidBackupCode(string code)
    {
        if (string.IsNullOrEmpty(BackupCodes)) return false;
        // Simple check - in reality, parse JSON and validate
        return BackupCodes.Contains(code);
    }

    public void UseBackupCode(string code)
    {
        // Implementation: Remove used code from BackupCodes JSON
        // This would update the BackupCodes property
    }

    // ===== SUBSCRIPTION METHODS =====

    public void UpgradeSubscription(string newTier)
    {
        var previousTier = SubscriptionTier;
        SubscriptionTier = newTier;

        // Update transaction limits based on subscription
        if (newTier == "Premium" || newTier == "Business")
        {
            DailyTransactionLimit *= 2;
            MonthlyTransactionLimit *= 2;
        }

        // Auto-assign financial advisor for premium tiers
        if ((newTier == "Premium" || newTier == "Business") && string.IsNullOrEmpty(AssignedFinancialAdvisor))
        {
            // This would assign an advisor in real implementation
        }
    }
    public User()
    {
        Id = Guid.NewGuid();

        // Explicitly set defaults for clarity 
        IsActive = true;
        EmailVerified = false;
        FailedLoginAttempts = 0;
        KycStatus = KycStatus.Pending;
        RiskLevel = RiskLevel.Low;
        DailyTransactionLimit = 10000.00m;
        MonthlyTransactionLimit = 50000.00m;
        IsFlaggedForReview = false;
        Currency = "R";
        Language = "en";
        SubscriptionTier = "Free";
        InvestmentRiskTolerance = RiskTolerance.Moderate;
        PrimaryInvestmentGoal = InvestmentGoal.Growth;
        RequiresPeriodicReview = false;
        ConsentGiven = false;
        MarketingOptIn = false;

        // Initialize collections
        UserRoles = new List<UserRole>();
        RefreshTokens = new List<RefreshToken>();
        PasswordHistories = new List<PasswordHistory>();
        LoginLogs = new List<LoginLog>();
        FailedLoginAttemptsLog = new List<FailedLoginAttempt>();
    }
}



// ===== SUPPORTING ENUMS =====
public enum KycStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2,
    UnderReview = 3
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Restricted = 4
}

public enum RiskTolerance
{
    Conservative = 1,
    Moderate = 2,
    Aggressive = 3
}

public enum InvestmentGoal
{
    CapitalPreservation = 1,
    Income = 2,
    Growth = 3,
    Speculation = 4
}


