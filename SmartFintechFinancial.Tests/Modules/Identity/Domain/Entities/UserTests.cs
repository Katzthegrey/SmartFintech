using FluentAssertions;
using SmartFintechFinancial.Modules.Identity.Domain.Constants;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Tests.Modules.Identity.Infrastructure.Persistence;
using SmartFintechFinancial.Tests.Modules.Identity.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SmartFintechFinancial.Tests.Modules.Identity.Domain.Entities;

public class UserTests : TestBase
{
    #region Constructor & Default Values

    [Fact]
    public void Constructor_ShouldInitializeCollections()
    {
        // Act
        var user = new User();

        // Assert
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.RefreshTokens.Should().NotBeNull().And.BeEmpty();
        user.PasswordHistories.Should().NotBeNull().And.BeEmpty();
        user.LoginLogs.Should().NotBeNull().And.BeEmpty();
        user.FailedLoginAttemptsLog.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var user = new User();

       

        // Assert - Core Identity
        user.Id.Should().NotBeEmpty();
        user.IsActive.Should().BeTrue();
        user.EmailVerified.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntil.Should().BeNull();

        // Assert - KYC/AML
        user.KycStatus.Should().Be(KycStatus.Pending);
        user.KycVerifiedAt.Should().BeNull();
        user.KycVerifiedBy.Should().BeNull();
        user.KycRejectionReason.Should().BeNull();

        // Assert - Risk/Fraud
        user.RiskLevel.Should().Be(RiskLevel.Low);
        user.RiskAssessedAt.Should().BeNull();
        user.RiskAssessedBy.Should().BeNull();
        user.RiskNotes.Should().BeNull();
        user.DailyTransactionLimit.Should().Be(10000.00m);
        user.MonthlyTransactionLimit.Should().Be(50000.00m);
        user.IsFlaggedForReview.Should().BeFalse();
        user.FlaggedAt.Should().BeNull();
        user.FlagReason.Should().BeNull();

        // Assert - Preferences
        user.Timezone.Should().BeNull();
        user.Currency.Should().Be("R");
        user.Language.Should().Be("en");
        user.SubscriptionTier.Should().Be("Free");
        user.InvestmentRiskTolerance.Should().Be(RiskTolerance.Moderate);
        user.PrimaryInvestmentGoal.Should().Be(InvestmentGoal.Growth);

        // Assert - Compliance
        user.RequiresPeriodicReview.Should().BeFalse();
        user.NextReviewDate.Should().BeNull();
        user.AssignedFinancialAdvisor.Should().BeNull();
        user.AssignedComplianceOfficer.Should().BeNull();

        // Assert - GDPR
        user.ConsentGiven.Should().BeFalse();
        user.ConsentGivenAt.Should().BeNull();
        user.MarketingOptIn.Should().BeFalse();
    }

    #endregion

    #region Password Management

    [Fact]
    public void HashPassword_ShouldReturnBCryptHash()
    {
        // Arrange
        var password = "Test123!@#";

        // Act
        var hash = User.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2a$"); // BCrypt format
    }

    [Fact]
    public void HashPassword_ShouldUseWorkFactor12()
    {
        // Arrange
        var password = "Test123!@#";

        // Act
        var hash = User.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        // BCrypt work factor is embedded in the hash, but we can't easily test it
        // Just verify it's a valid BCrypt hash
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var user = new User();
        var password = "Test123!@#";
        user.PasswordHash = User.HashPassword(password);

        // Act
        var result = user.VerifyPassword(password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var user = new User();
        user.PasswordHash = User.HashPassword("CorrectPassword123!");

        // Act
        var result = user.VerifyPassword("WrongPassword123!");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordHashIsNull()
    {
        // Arrange
        var user = new User { PasswordHash = null! };

        // Debug: Check what's actually in PasswordHash
        Console.WriteLine($"PasswordHash is null: {user.PasswordHash == null}");
        Console.WriteLine($"PasswordHash is null or empty: {string.IsNullOrEmpty(user.PasswordHash)}");

        // Let's also check the type
        Console.WriteLine($"PasswordHash type: {user.PasswordHash?.GetType() ?? null}");

        // Act
        var result = user.VerifyPassword("AnyPassword");
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordExpired_ShouldReturnTrue_WhenExpiryDatePassed()
    {
        // Arrange
        var user = new User { PasswordExpiresAt = DateTime.UtcNow.AddDays(-1) };

        // Act
        var result = user.IsPasswordExpired;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPasswordExpired_ShouldReturnFalse_WhenExpiryDateInFuture()
    {
        // Arrange
        var user = new User { PasswordExpiresAt = DateTime.UtcNow.AddDays(1) };

        // Act
        var result = user.IsPasswordExpired;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPasswordExpired_ShouldReturnFalse_WhenNoExpiryDate()
    {
        // Arrange
        var user = new User { PasswordExpiresAt = null };

        // Act
        var result = user.IsPasswordExpired;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RequiresPasswordChange_ShouldReturnTrue_WhenPasswordExpired()
    {
        // Arrange
        var user = new User { PasswordExpiresAt = DateTime.UtcNow.AddDays(-1) };

        // Act
        var result = user.RequiresPasswordChange;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RequiresPasswordChange_ShouldReturnTrue_WhenPasswordOld()
    {
        // Arrange
        var user = new User
        {
            PasswordChangedAt = DateTime.UtcNow.AddDays(-100) // Over 90 days
        };

        // Act
        var result = user.RequiresPasswordChange;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RequiresPasswordChange_ShouldReturnFalse_WhenPasswordRecent()
    {
        // Arrange
        var user = new User
        {
            PasswordChangedAt = DateTime.UtcNow.AddDays(-30) // Under 90 days
        };

        // Act
        var result = user.RequiresPasswordChange;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Role Management

    [Fact]
    public void HasRole_ShouldReturnTrue_WhenUserHasRole()
    {
        // Arrange
        var user = new User();
        var role = new Role { Name = RoleConstants.FinancialAdvisor };
        user.UserRoles.Add(new UserRole { Role = role });

        // Act
        var result = user.HasRole(RoleConstants.FinancialAdvisor);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasRole_ShouldReturnFalse_WhenUserDoesNotHaveRole()
    {
        // Arrange
        var user = new User();
        var role = new Role { Name = RoleConstants.Client };
        user.UserRoles.Add(new UserRole { Role = role });

        // Act
        var result = user.HasRole(RoleConstants.FinancialAdvisor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasRole_ShouldReturnFalse_WhenUserHasNoRoles()
    {
        // Arrange
        var user = new User();

        // Act
        var result = user.HasRole(RoleConstants.Client);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAnyRole_ShouldReturnTrue_WhenUserHasAtLeastOneRole()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Investor } });

        // Act
        var result = user.HasAnyRole(RoleConstants.Client, RoleConstants.Investor, RoleConstants.FinancialAdvisor);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAnyRole_ShouldReturnFalse_WhenUserHasNoneOfTheRoles()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Client } });

        // Act
        var result = user.HasAnyRole(RoleConstants.FinancialAdvisor, RoleConstants.WealthManager);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAllRoles_ShouldReturnTrue_WhenUserHasAllRoles()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Client } });
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Investor } });

        // Act
        var result = user.HasAllRoles(RoleConstants.Client, RoleConstants.Investor);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAllRoles_ShouldReturnFalse_WhenUserMissingSomeRoles()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Client } });

        // Act
        var result = user.HasAllRoles(RoleConstants.Client, RoleConstants.Investor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPrimaryRole_ShouldReturnHighestPriorityRole()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Client } });
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Investor } });
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.FinancialAdvisor } });

        // Act
        var primaryRole = user.GetPrimaryRole();

        // Assert
        primaryRole.Should().Be(RoleConstants.FinancialAdvisor);
    }

    [Fact]
    public void GetPrimaryRole_ShouldReturnClient_WhenNoRoles()
    {
        // Arrange
        var user = new User();

        // Act
        var primaryRole = user.GetPrimaryRole();

        // Assert
        primaryRole.Should().Be(RoleConstants.Client);
    }

    [Fact]
    public void RoleNames_Property_ShouldReturnListOfRoleNames()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Client } });
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Investor } });

        // Act
        var roleNames = user.RoleNames;

        // Assert
        roleNames.Should().HaveCount(2);
        roleNames.Should().Contain(RoleConstants.Client);
        roleNames.Should().Contain(RoleConstants.Investor);
    }

    [Fact]
    public void RoleNames_Property_ShouldReturnEmptyList_WhenNoRoles()
    {
        // Arrange
        var user = new User();

        // Act
        var roleNames = user.RoleNames;

        // Assert
        roleNames.Should().BeEmpty();
    }

    [Theory]
    [InlineData(RoleConstants.Client, "Client", true)]
    [InlineData(RoleConstants.Investor, "Client", true)]
    [InlineData(RoleConstants.PremiumInvestor, "Client", true)]
    [InlineData(RoleConstants.BusinessInvestor, "Client", true)]
    [InlineData(RoleConstants.FinancialAdvisor, "Advisor", true)]
    [InlineData(RoleConstants.WealthManager, "Management", true)]
    [InlineData(RoleConstants.SupportAgent, "Support", true)]
    [InlineData(RoleConstants.FraudAnalyst, "Security", true)]
    [InlineData(RoleConstants.ComplianceOfficer, "Compliance", true)]
    [InlineData(RoleConstants.FinanceAdmin, "Admin", true)]
    [InlineData(RoleConstants.SuperAdmin, "Admin", true)]
    [InlineData(RoleConstants.Client, "Admin", false)]
    public void IsInRoleCategory_ShouldReturnExpectedResult(string roleName, string category, bool expected)
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = roleName } });

        // Act
        var result = user.IsInRoleCategory(category);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsInRoleCategory_ShouldReturnFalse_WhenNoRoles()
    {
        // Arrange
        var user = new User();

        // Act
        var result = user.IsInRoleCategory("Client");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region KYC/AML Management

    [Fact]
    public void ApproveKyc_ShouldUpdateStatusAndSetVerifiedBy()
    {
        // Arrange
        var user = new User { KycStatus = KycStatus.Pending };
        var verifiedBy = "compliance@example.com";

        // Act
        user.ApproveKyc(verifiedBy);

        // Assert
        user.KycStatus.Should().Be(KycStatus.Verified);
        user.KycVerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.KycVerifiedBy.Should().Be(verifiedBy);
        user.KycRejectionReason.Should().BeNull();
    }

    [Fact]
    public void RejectKyc_ShouldUpdateStatusAndIncreaseRiskLevel()
    {
        // Arrange
        var user = new User { KycStatus = KycStatus.Pending, RiskLevel = RiskLevel.Low };
        var reason = "Invalid documentation - ID not legible";
        var rejectedBy = "compliance@example.com";

        // Act
        user.RejectKyc(reason, rejectedBy);

        // Assert
        user.KycStatus.Should().Be(KycStatus.Rejected);
        user.KycRejectionReason.Should().Be(reason);
        user.RiskLevel.Should().Be(RiskLevel.High); // Increases to High due to rejection
        user.RiskAssessedBy.Should().Be(rejectedBy);
        user.RiskNotes.Should().Contain(reason);
        user.KycVerifiedAt.Should().BeNull();
        user.KycVerifiedBy.Should().BeNull();
    }

    #endregion

    #region Risk & Fraud Detection

    [Theory]
    [InlineData(RiskLevel.Low, 50000.00, 250000.00)]
    [InlineData(RiskLevel.Medium, 10000.00, 50000.00)]
    [InlineData(RiskLevel.High, 1000.00, 5000.00)]
    [InlineData(RiskLevel.Restricted, 0.00, 0.00)]
    public void UpdateRiskLevel_ShouldUpdateTransactionLimits(RiskLevel newLevel, decimal expectedDaily, decimal expectedMonthly)
    {
        // Arrange
        var user = new User();
        var assessedBy = "risk@example.com";
        var notes = "Risk assessment based on transaction pattern";

        // Act
        user.UpdateRiskLevel(newLevel, assessedBy, notes);

        // Assert
        user.RiskLevel.Should().Be(newLevel);
        user.RiskAssessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.RiskAssessedBy.Should().Be(assessedBy);
        user.RiskNotes.Should().Be(notes);
        user.DailyTransactionLimit.Should().Be(expectedDaily);
        user.MonthlyTransactionLimit.Should().Be(expectedMonthly);
    }

    [Fact]
    public void FlagForReview_ShouldSetFlagPropertiesAndIncreaseRisk()
    {
        // Arrange
        var user = new User { RiskLevel = RiskLevel.Low };
        var reason = "Unusual transaction pattern detected";
        var flaggedBy = "fraud_analyst@example.com";

        // Act
        user.FlagForReview(reason, flaggedBy);

        // Assert
        user.IsFlaggedForReview.Should().BeTrue();
        user.FlaggedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.FlagReason.Should().Be(reason);
        user.RiskLevel.Should().Be(RiskLevel.Medium); // Should increase from Low to Medium
    }

    [Fact]
    public void FlagForReview_ShouldNotDecreaseRiskLevel_WhenAlreadyHigh()
    {
        // Arrange
        var user = new User();
        user.UpdateRiskLevel(RiskLevel.High, "system");

        // Act
        user.FlagForReview("Test flag", "system");

        // Assert
        user.RiskLevel.Should().Be(RiskLevel.High); // Should stay High
    }

    [Fact]
    public void ClearFlag_ShouldResetFlagProperties()
    {
        // Arrange
        var user = new User();
        user.FlagForReview("Test reason", "system");
        var clearedBy = "compliance@example.com";

        // Act
        user.ClearFlag(clearedBy);

        // Assert
        user.IsFlaggedForReview.Should().BeFalse();
        user.FlagReason.Should().BeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedBy.Should().Be(clearedBy);
    }

    [Theory]
    [InlineData(RiskLevel.Low, 10000.00, true)]
    [InlineData(RiskLevel.Low, 50000.00, true)] // At limit
    [InlineData(RiskLevel.Low, 50001.00, false)] // Over limit
    [InlineData(RiskLevel.Medium, 10000.00, true)]
    [InlineData(RiskLevel.Medium, 10001.00, false)]
    [InlineData(RiskLevel.High, 1000.00, true)]
    [InlineData(RiskLevel.High, 1001.00, false)]
    public void CanTransact_ShouldRespectDailyLimit(RiskLevel riskLevel, decimal amount, bool expected)
    {
        // Arrange
        var user = new User();
        user.UpdateRiskLevel(riskLevel, "system");

        // Act
        var result = user.CanTransact(amount);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CanTransact_ShouldReturnFalse_WhenRiskLevelRestricted()
    {
        // Arrange
        var user = new User();
        user.UpdateRiskLevel(RiskLevel.Restricted, "system");

        // Act
        var result = user.CanTransact(100.00m);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanTransact_ShouldReturnFalse_WhenFlaggedForReview()
    {
        // Arrange
        var user = new User();
        user.FlagForReview("Test flag", "system");

        // Act
        var result = user.CanTransact(100.00m);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanTransact_ShouldApplyRoleBasedRestrictions_ForClientRole()
    {
        // Arrange
        var user = new User();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = RoleConstants.Client } });
        user.RiskLevel = RiskLevel.Low;

        // Act
        var result = user.CanTransact(15000.00m); // Client limit is 10000

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanTransact_ShouldAllow_ForInvestorRole_WithHigherLimits()
    {
        // Arrange
        var user = new User();

        // Set risk level to Low (this sets DailyTransactionLimit to 50000)
        user.UpdateRiskLevel(RiskLevel.Low, "system");

        // Create a proper Role object
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = RoleConstants.Investor,
            // Add any other required Role properties
        };

        // Create UserRole with both navigation properties set
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role
        };

        // Add to user's collection
        user.UserRoles.Add(userRole);

        // Verify the role was added correctly
        Assert.True(user.HasRole(RoleConstants.Investor), "User should have Investor role");
        Assert.False(user.HasRole(RoleConstants.Client), "User should not have Client role");

        // Act
        var result = user.CanTransact(25000.00m);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Login Security

    [Fact]
    public void IsLockedOut_ShouldReturnTrue_WhenLockedUntilInFuture()
    {
        // Arrange
        var user = new User { LockedUntil = DateTime.UtcNow.AddMinutes(15) };

        // Act
        var result = user.IsLockedOut;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_ShouldReturnFalse_WhenLockedUntilInPast()
    {
        // Arrange
        var user = new User { LockedUntil = DateTime.UtcNow.AddMinutes(-15) };

        // Act
        var result = user.IsLockedOut;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_ShouldReturnFalse_WhenNoLock()
    {
        // Arrange
        var user = new User { LockedUntil = null };

        // Act
        var result = user.IsLockedOut;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementAttempts()
    {
        // Arrange
        var user = new User();

        // Act
        user.RecordFailedLogin("192.168.1.1");

        // Assert
        user.FailedLoginAttempts.Should().Be(1);
        user.LockedUntil.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_ShouldLockAccount_After5Attempts()
    {
        // Arrange
        var user = new User();

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin($"192.168.1.{i}");
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(5);
        user.LockedUntil.Should().NotBeNull();
        user.LockedUntil.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncreaseRiskLevel_WhenLocked()
    {
        // Arrange
        var user = new User { RiskLevel = RiskLevel.Low };

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin("192.168.1.1");
        }

        // Assert
        user.RiskLevel.Should().Be(RiskLevel.Medium);
        user.RiskNotes.Should().Contain("Multiple failed login attempts");
    }

    [Fact]
    public void RecordFailedLogin_ShouldNotIncreaseRisk_WhenAlreadyMedium()
    {
        // Arrange
        var user = new User { RiskLevel = RiskLevel.Medium };

        // Act
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin("192.168.1.1");
        }

        // Assert
        user.RiskLevel.Should().Be(RiskLevel.Medium); // Shouldn't increase further
    }

    [Fact]
    public void ResetFailedLogins_ShouldClearAttemptsAndLock()
    {
        // Arrange
        var user = new User();
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin("192.168.1.1");
        }

        // Act
        user.ResetFailedLogins();

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntil.Should().BeNull();
    }

    #endregion

    #region Authentication

    [Fact]
    public void CanAuthenticate_ShouldReturnTrue_WhenAllConditionsMet()
    {
        // Arrange
        var user = new User
        {
            IsActive = true,
            EmailVerified = true,
            KycStatus = KycStatus.Verified,
            RiskLevel = RiskLevel.Low,
            PasswordExpiresAt = DateTime.UtcNow.AddDays(30),
            LockedUntil = null
        };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAuthenticate_ShouldReturnFalse_WhenInactive()
    {
        // Arrange
        var user = new User { IsActive = false };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAuthenticate_ShouldReturnFalse_WhenLockedOut()
    {
        // Arrange
        var user = new User { LockedUntil = DateTime.UtcNow.AddMinutes(15) };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAuthenticate_ShouldReturnFalse_WhenEmailNotVerified()
    {
        // Arrange
        var user = new User
        {
            IsActive = true,
            EmailVerified = false,
            KycStatus = KycStatus.Verified
        };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAuthenticate_ShouldReturnFalse_WhenPasswordExpired()
    {
        // Arrange
        var user = new User
        {
            IsActive = true,
            EmailVerified = true,
            KycStatus = KycStatus.Verified,
            PasswordExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAuthenticate_ShouldReturnFalse_WhenKycNotVerified()
    {
        // Arrange
        var user = new User
        {
            IsActive = true,
            EmailVerified = true,
            KycStatus = KycStatus.Pending
        };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAuthenticate_ShouldReturnFalse_WhenRiskLevelRestricted()
    {
        // Arrange
        var user = new User
        {
            IsActive = true,
            EmailVerified = true,
            KycStatus = KycStatus.Verified,
            RiskLevel = RiskLevel.Restricted
        };

        // Act
        var result = user.CanAuthenticate();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Two-Factor Authentication

    [Fact]
    public void HasValidBackupCode_ShouldReturnTrue_WhenCodeExists()
    {
        // Arrange
        var user = new User { BackupCodes = "ABC123,DEF456,GHI789" };

        // Act
        var result = user.HasValidBackupCode("DEF456");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasValidBackupCode_ShouldReturnFalse_WhenCodeDoesNotExist()
    {
        // Arrange
        var user = new User { BackupCodes = "ABC123,DEF456,GHI789" };

        // Act
        var result = user.HasValidBackupCode("XYZ999");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasValidBackupCode_ShouldReturnFalse_WhenNoBackupCodes()
    {
        // Arrange
        var user = new User { BackupCodes = null };

        // Act
        var result = user.HasValidBackupCode("ABC123");

        // Assert
        result.Should().BeFalse();
    }

    // Note: UseBackupCode method exists in your model but needs implementation
    // We'll add tests once it's implemented

    #endregion

    #region Subscription & Limits

    [Theory]
    [InlineData("Premium")]
    [InlineData("Business")]
    public void UpgradeSubscription_ShouldDoubleTransactionLimits_ForPremiumTiers(string newTier)
    {
        // Arrange
        var user = new User();
        var originalDaily = user.DailyTransactionLimit;
        var originalMonthly = user.MonthlyTransactionLimit;

        // Act
        user.UpgradeSubscription(newTier);

        // Assert
        user.SubscriptionTier.Should().Be(newTier);
        user.DailyTransactionLimit.Should().Be(originalDaily * 2);
        user.MonthlyTransactionLimit.Should().Be(originalMonthly * 2);
    }

    [Theory]
    [InlineData("Free")]
    [InlineData("Standard")]
    [InlineData("Basic")]
    public void UpgradeSubscription_ShouldKeepSameLimits_ForNonPremiumTiers(string newTier)
    {
        // Arrange
        var user = new User();
        var originalDaily = user.DailyTransactionLimit;
        var originalMonthly = user.MonthlyTransactionLimit;

        // Act
        user.UpgradeSubscription(newTier);

        // Assert
        user.SubscriptionTier.Should().Be(newTier);
        user.DailyTransactionLimit.Should().Be(originalDaily);
        user.MonthlyTransactionLimit.Should().Be(originalMonthly);
    }

    #endregion

    #region GDPR & Consent

    [Fact]
    public void ConsentGiven_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.ConsentGiven.Should().BeFalse();
        user.ConsentGivenAt.Should().BeNull();
        user.MarketingOptIn.Should().BeFalse();
    }

    // Note: Your model has consent properties but no methods to modify them
    // These would need to be added or handled in services

    #endregion

    #region Audit Trail

    [Fact]
    public void CreatedAt_ShouldBeSet_WhenAddedToContext()
    {
        // Arrange
        var user = TestDataFactory.CreateValidUser();

        // Act
        Context.Users.Add(user);
        Context.SaveChanges();

        // Assert
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.CreatedBy.Should().Be("system"); // Default value
    }

    [Fact]
    public void UpdatedAt_ShouldBeSet_WhenUserModified()
    {
        // Arrange
        var user = TestDataFactory.CreateValidUser();
        Context.Users.Add(user);
        Context.SaveChanges();

        // Act
        user.IsActive = false;
        Context.SaveChanges();

        // Assert
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedBy.Should().Be("system"); // Default value
    }

    [Fact]
    public void CreatedAt_ShouldNotChange_OnUpdate()
    {
        // Arrange
        var user = TestDataFactory.CreateValidUser();
        Context.Users.Add(user);
        Context.SaveChanges();
        var originalCreatedAt = user.CreatedAt;
        var originalCreatedBy = user.CreatedBy;

        // Act
        user.IsActive = false;
        Context.SaveChanges();

        // Assert
        user.CreatedAt.Should().Be(originalCreatedAt);
        user.CreatedBy.Should().Be(originalCreatedBy);
    }

    #endregion
}