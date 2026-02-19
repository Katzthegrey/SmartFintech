using SmartFintechFinancial.Modules.Identity.Domain.Entities;

namespace SmartFintechFinancial.Tests.Modules.Identity.TestHelpers;

public static class TestDataFactory
{
    public static User CreateValidUser(string email = "test@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = User.HashPassword("Test123!@#"),
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            EmailVerified = true,
            KycStatus = KycStatus.Verified,
            RiskLevel = RiskLevel.Low,
            Currency = "ZAR",
            Language = "en",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    public static RefreshToken CreateValidRefreshToken(Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsRevoked = false
        };
    }

    public static FailedLoginAttempt CreateFailedLoginAttempt(Guid? userId = null, string ip = "127.0.0.1")
    {
        return new FailedLoginAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = userId.HasValue ? null : "unknown@example.com",
            IpAddress = ip,
            UserAgent = "Mozilla/5.0 Test",
            Reason = "Invalid password",
            AttemptNumber = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    public static LoginLog CreateLoginLog(Guid userId, bool success = true)
    {
        return new LoginLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = "127.0.0.1",
            UserAgent = "Mozilla/5.0 Test",
            IsSuccess = success,
            FailureReason = success ? null : "Invalid password",
            TwoFactorUsed = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }

    public static PasswordHistory CreatePasswordHistory(Guid userId)
    {
        return new PasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = User.HashPassword("NewPassword123!@#"),
            ChangedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };
    }
}