using FluentAssertions;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Tests.Modules.Identity.Infrastructure.Persistence;
using SmartFintechFinancial.Tests.Modules.Identity.TestHelpers;
using System;
using System.Linq;
using Xunit;

namespace SmartFintechFinancial.Tests.Modules.Identity.Domain.Entities;

public class FailedLoginAttemptTests : TestBase
{
    #region Constructor & Default Values

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.Id.Should().Be(Guid.Empty);
        attempt.UserId.Should().BeNull();
        attempt.User.Should().BeNull();
        attempt.Email.Should().BeNull();
        attempt.IpAddress.Should().BeEmpty();
        attempt.Reason.Should().BeEmpty();
        attempt.AttemptNumber.Should().Be(0);
        attempt.UserAgent.Should().BeEmpty();
        attempt.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        attempt.CreatedBy.Should().Be("system");
        attempt.UpdatedAt.Should().BeNull();
        attempt.UpdatedBy.Should().Be("system");
    }

    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var ipAddress = "192.168.1.100";
        var reason = "Invalid password";
        var attemptNumber = 3;
        var userAgent = "Mozilla/5.0";

        // Act
        var attempt = new FailedLoginAttempt
        {
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            Reason = reason,
            AttemptNumber = attemptNumber,
            UserAgent = userAgent
        };

        // Assert
        attempt.UserId.Should().Be(userId);
        attempt.Email.Should().Be(email);
        attempt.IpAddress.Should().Be(ipAddress);
        attempt.Reason.Should().Be(reason);
        attempt.AttemptNumber.Should().Be(attemptNumber);
        attempt.UserAgent.Should().Be(userAgent);
        attempt.CreatedBy.Should().Be("system");
        attempt.UpdatedBy.Should().Be("system");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Id_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var attempt = new FailedLoginAttempt();

        // Act
        attempt.Id = id;

        // Assert
        attempt.Id.Should().Be(id);
    }

    [Fact]
    public void UserId_ShouldBeNullable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var userId = Guid.NewGuid();

        // Act
        attempt.UserId = userId;

        // Assert
        attempt.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.UserId.Should().BeNull();
    }

    [Fact]
    public void Email_ShouldBeNullable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var email = "test@example.com";

        // Act
        attempt.Email = email;

        // Assert
        attempt.Email.Should().Be(email);
    }

    [Fact]
    public void Email_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.Email.Should().BeNull();
    }

    [Fact]
    public void IpAddress_ShouldBeSettableAndGettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var ipAddress = "192.168.1.100";

        // Act
        attempt.IpAddress = ipAddress;

        // Assert
        attempt.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public void IpAddress_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.IpAddress.Should().BeEmpty();
    }

    [Fact]
    public void Reason_ShouldBeSettableAndGettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var reason = "Invalid password";

        // Act
        attempt.Reason = reason;

        // Assert
        attempt.Reason.Should().Be(reason);
    }

    [Fact]
    public void Reason_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.Reason.Should().BeEmpty();
    }

    [Fact]
    public void AttemptNumber_ShouldBeSettableAndGettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var attemptNumber = 5;

        // Act
        attempt.AttemptNumber = attemptNumber;

        // Assert
        attempt.AttemptNumber.Should().Be(attemptNumber);
    }

    [Fact]
    public void AttemptNumber_ShouldDefaultToZero()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.AttemptNumber.Should().Be(0);
    }

    [Fact]
    public void UserAgent_ShouldBeSettableAndGettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        // Act
        attempt.UserAgent = userAgent;

        // Assert
        attempt.UserAgent.Should().Be(userAgent);
    }

    [Fact]
    public void UserAgent_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.UserAgent.Should().BeEmpty();
    }

    #endregion

    #region IsRecent Method Tests

    [Fact]
    public void IsRecent_ShouldReturnTrue_WhenWithinTimeWindow()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        attempt.CreatedAt = DateTime.UtcNow.AddMinutes(-5);
        var timeWindow = TimeSpan.FromMinutes(10);

        // Act
        var result = attempt.IsRecent(timeWindow);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRecent_ShouldReturnFalse_WhenOutsideTimeWindow()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        attempt.CreatedAt = DateTime.UtcNow.AddMinutes(-15);
        var timeWindow = TimeSpan.FromMinutes(10);

        // Act
        var result = attempt.IsRecent(timeWindow);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRecent_ShouldReturnFalse_WhenExactlyAtWindowBoundary()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        attempt.CreatedAt = DateTime.UtcNow.AddMinutes(-10);
        var timeWindow = TimeSpan.FromMinutes(10);

        // Act
        var result = attempt.IsRecent(timeWindow);

        // Assert
        result.Should().BeFalse(); // Exactly at boundary is NOT recent
    }

    [Fact]
    public void IsRecent_ShouldHandle_ZeroTimeWindow()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        attempt.CreatedAt = DateTime.UtcNow.AddSeconds(-1);
        var timeWindow = TimeSpan.Zero;

        // Act
        var result = attempt.IsRecent(timeWindow);

        // Assert
        result.Should().BeFalse(); // 1 second ago > 0
    }

    [Fact]
    public void IsRecent_ShouldHandle_VeryLargeTimeWindow()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        attempt.CreatedAt = DateTime.UtcNow.AddDays(-30);
        var timeWindow = TimeSpan.FromDays(365);

        // Act
        var result = attempt.IsRecent(timeWindow);

        // Assert
        result.Should().BeTrue(); // 30 days < 365 days
    }

    [Fact]
    public void IsRecent_ShouldHandle_FutureCreatedAt()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        attempt.CreatedAt = DateTime.UtcNow.AddHours(1); // Future (shouldn't happen in reality)
        var timeWindow = TimeSpan.FromMinutes(30);

        // Act
        var result = attempt.IsRecent(timeWindow);

        // Assert
        result.Should().BeFalse(); // Future date > now
    }

    #endregion

    #region User Relationship Tests

    [Fact]
    public void User_ShouldBeSettableAndGettable()
    {
        // Arrange
        var user = new User();
        var attempt = new FailedLoginAttempt();

        // Act
        attempt.User = user;

        // Assert
        attempt.User.Should().Be(user);
    }

    [Fact]
    public void User_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.User.Should().BeNull();
    }

    [Fact]
    public void UserId_And_User_ShouldBeConsistent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var attempt = new FailedLoginAttempt
        {
            UserId = userId,
            User = user
        };

        // Assert
        attempt.UserId.Should().Be(user.Id);
        attempt.User.Should().Be(user);
    }

    #endregion

    #region Audit Trail Tests

    [Fact]
    public void CreatedAt_ShouldDefaultToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.CreatedAt.Should().BeAfter(beforeCreation);
        attempt.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var customDate = DateTime.UtcNow.AddDays(-1);

        // Act
        attempt.CreatedAt = customDate;

        // Assert
        attempt.CreatedAt.Should().Be(customDate);
    }

    [Fact]
    public void CreatedBy_ShouldDefaultToSystem()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.CreatedBy.Should().Be("system");
    }

    [Fact]
    public void CreatedBy_ShouldBeSettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var createdBy = "test-user";

        // Act
        attempt.CreatedBy = createdBy;

        // Assert
        attempt.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void UpdatedAt_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var updatedAt = DateTime.UtcNow;

        // Act
        attempt.UpdatedAt = updatedAt;

        // Assert
        attempt.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdatedBy_ShouldDefaultToSystem()
    {
        // Arrange & Act
        var attempt = new FailedLoginAttempt();

        // Assert
        attempt.UpdatedBy.Should().Be("system");
    }

    [Fact]
    public void UpdatedBy_ShouldBeSettable()
    {
        // Arrange
        var attempt = new FailedLoginAttempt();
        var updatedBy = "audit-user";

        // Act
        attempt.UpdatedBy = updatedBy;

        // Assert
        attempt.UpdatedBy.Should().Be(updatedBy);
    }

    #endregion

    #region Database Persistence Tests

    [Fact]
    public async Task CanSaveAndRetrieve_FailedLoginAttempt_WithUser()
    {
        // Arrange
        var user = TestDataFactory.CreateValidUser();
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var attempt = new FailedLoginAttempt
        {
            UserId = user.Id,
            Email = user.Email,
            IpAddress = "192.168.1.100",
            Reason = "Invalid password",
            AttemptNumber = 2,
            UserAgent = "Mozilla/5.0",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        // Act
        Context.FailedLoginAttempts.Add(attempt);
        await Context.SaveChangesAsync();

        // Assert
        var savedAttempt = await Context.FailedLoginAttempts.FindAsync(attempt.Id);
        savedAttempt.Should().NotBeNull();
        savedAttempt!.UserId.Should().Be(user.Id);
        savedAttempt.Email.Should().Be(user.Email);
        savedAttempt.IpAddress.Should().Be("192.168.1.100");
        savedAttempt.Reason.Should().Be("Invalid password");
        savedAttempt.AttemptNumber.Should().Be(2);
        savedAttempt.UserAgent.Should().Be("Mozilla/5.0");
    }

    [Fact]
    public async Task CanSaveAndRetrieve_FailedLoginAttempt_WithoutUser()
    {
        // Arrange
        var attempt = new FailedLoginAttempt
        {
            UserId = null,
            Email = "unknown@example.com",
            IpAddress = "192.168.1.100",
            Reason = "User not found",
            AttemptNumber = 1,
            UserAgent = "Mozilla/5.0"
        };

        // Act
        Context.FailedLoginAttempts.Add(attempt);
        await Context.SaveChangesAsync();

        // Assert
        var savedAttempt = await Context.FailedLoginAttempts.FindAsync(attempt.Id);
        savedAttempt.Should().NotBeNull();
        savedAttempt!.UserId.Should().BeNull();
        savedAttempt.Email.Should().Be("unknown@example.com");
        savedAttempt.IpAddress.Should().Be("192.168.1.100");
    }

   

    [Fact]
    public async Task CanDelete_FailedLoginAttempt()
    {
        // Arrange
        var attempt = new FailedLoginAttempt
        {
            IpAddress = "192.168.1.100",
            Reason = "Test delete"
        };

        Context.FailedLoginAttempts.Add(attempt);
        await Context.SaveChangesAsync();

        // Act
        Context.FailedLoginAttempts.Remove(attempt);
        await Context.SaveChangesAsync();

        // Assert
        var deletedAttempt = await Context.FailedLoginAttempts.FindAsync(attempt.Id);
        deletedAttempt.Should().BeNull();
    }

    #endregion

    #region User Cascade Delete Test

    [Fact]
    public async Task DeletingUser_ShouldCascadeDelete_FailedLoginAttempts()
    {
        // Arrange
        var user = TestDataFactory.CreateValidUser();

        var attempt1 = new FailedLoginAttempt
        {
            UserId = user.Id,
            IpAddress = "192.168.1.100",
            Reason = "Failed attempt 1"
        };

        var attempt2 = new FailedLoginAttempt
        {
            UserId = user.Id,
            IpAddress = "192.168.1.101",
            Reason = "Failed attempt 2"
        };

        user.FailedLoginAttemptsLog.Add(attempt1);
        user.FailedLoginAttemptsLog.Add(attempt2);

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var attempts = Context.FailedLoginAttempts.ToList();
        attempts.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MultipleAttempts_FromSameIp_ShouldBeAllowed()
    {
        // Arrange
        var ipAddress = "192.168.1.100";

        var attempt1 = new FailedLoginAttempt { IpAddress = ipAddress, AttemptNumber = 1 };
        var attempt2 = new FailedLoginAttempt { IpAddress = ipAddress, AttemptNumber = 2 };
        var attempt3 = new FailedLoginAttempt { IpAddress = ipAddress, AttemptNumber = 3 };

        // Act
        Context.FailedLoginAttempts.AddRange(attempt1, attempt2, attempt3);
        Context.SaveChanges();

        // Assert
        var ipAttempts = Context.FailedLoginAttempts
            .Where(a => a.IpAddress == ipAddress)
            .ToList();

        ipAttempts.Should().HaveCount(3);
    }

    [Fact]
    public void VeryLongReason_ShouldBeAllowed()
    {
        // Arrange
        var longReason = new string('X', 1000); // 1000 character reason
        var attempt = new FailedLoginAttempt { Reason = longReason };

        // Act
        Context.FailedLoginAttempts.Add(attempt);
        Context.SaveChanges();

        // Assert
        var savedAttempt = Context.FailedLoginAttempts.Find(attempt.Id);
        savedAttempt!.Reason.Should().HaveLength(1000);
    }

    [Fact]
    public void VeryLongUserAgent_ShouldBeAllowed()
    {
        // Arrange
        var longUserAgent = new string('Y', 2000); // 2000 character user agent
        var attempt = new FailedLoginAttempt { UserAgent = longUserAgent };

        // Act
        Context.FailedLoginAttempts.Add(attempt);
        Context.SaveChanges();

        // Assert
        var savedAttempt = Context.FailedLoginAttempts.Find(attempt.Id);
        savedAttempt!.UserAgent.Should().HaveLength(2000);
    }

    #endregion
}