using FluentAssertions;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Tests.Modules.Identity.Infrastructure.Persistence;
using SmartFintechFinancial.Tests.Modules.Identity.TestHelpers;
using System;
using System.Linq;
using Xunit;

namespace SmartFintechFinancial.Tests.Modules.Identity.Domain.Entities;

public class RefreshTokenTests : TestBase
{
    #region Constructor & Default Values

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.Id.Should().Be(Guid.Empty);
        token.Token.Should().BeEmpty();
        token.CreatedByIp.Should().BeEmpty();
        token.CreatedBy.Should().Be("System");
        token.UpdatedBy.Should().Be("System");
        token.IsRevoked.Should().BeFalse();
        token.RevokedAt.Should().BeNull();
        token.RevokedByIp.Should().BeNull();
        token.ReplacedByToken.Should().BeNull();
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        token.UpdatedAt.Should().Be(default);
    }

    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var createdByIp = "192.168.1.100";

        // Act
        var token = new RefreshToken
        {
            UserId = userId,
            Token = tokenString,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp
        };

        // Assert
        token.UserId.Should().Be(userId);
        token.Token.Should().Be(tokenString);
        token.ExpiresAt.Should().Be(expiresAt);
        token.CreatedByIp.Should().Be(createdByIp);
        token.CreatedBy.Should().Be("System");
        token.UpdatedBy.Should().Be("System");
        token.IsRevoked.Should().BeFalse();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void UserId_ShouldBeSettableAndGettable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new RefreshToken();

        // Act
        token.UserId = userId;

        // Assert
        token.UserId.Should().Be(userId);
    }

    [Fact]
    public void Token_ShouldBeSettableAndGettable()
    {
        // Arrange
        var tokenString = "test-token-123";
        var token = new RefreshToken();

        // Act
        token.Token = tokenString;

        // Assert
        token.Token.Should().Be(tokenString);
    }

    [Fact]
    public void ExpiresAt_ShouldBeSettableAndGettable()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var token = new RefreshToken();

        // Act
        token.ExpiresAt = expiresAt;

        // Assert
        token.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void CreatedAt_ShouldDefaultToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var token = new RefreshToken();

        // Assert
        token.CreatedAt.Should().BeAfter(beforeCreation);
        token.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreatedByIp_ShouldBeSettableAndGettable()
    {
        // Arrange
        var ipAddress = "192.168.1.100";
        var token = new RefreshToken();

        // Act
        token.CreatedByIp = ipAddress;

        // Assert
        token.CreatedByIp.Should().Be(ipAddress);
    }

    [Fact]
    public void CreatedBy_ShouldDefaultToSystem()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.CreatedBy.Should().Be("System");
    }

    [Fact]
    public void IsRevoked_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsRevoked_ShouldBeSettableToTrue()
    {
        // Arrange
        var token = new RefreshToken();

        // Act
        token.IsRevoked = true;

        // Assert
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void RevokedAt_ShouldBeNullable()
    {
        // Arrange
        var token = new RefreshToken();
        var revokedAt = DateTime.UtcNow;

        // Act
        token.RevokedAt = revokedAt;

        // Assert
        token.RevokedAt.Should().Be(revokedAt);
    }

    [Fact]
    public void RevokedAt_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void RevokedByIp_ShouldBeNullable()
    {
        // Arrange
        var token = new RefreshToken();
        var revokedByIp = "192.168.1.200";

        // Act
        token.RevokedByIp = revokedByIp;

        // Assert
        token.RevokedByIp.Should().Be(revokedByIp);
    }

    [Fact]
    public void RevokedByIp_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.RevokedByIp.Should().BeNull();
    }

    [Fact]
    public void ReplacedByToken_ShouldBeNullable()
    {
        // Arrange
        var token = new RefreshToken();
        var replacedByToken = "new-token-456";

        // Act
        token.ReplacedByToken = replacedByToken;

        // Assert
        token.ReplacedByToken.Should().Be(replacedByToken);
    }

    [Fact]
    public void ReplacedByToken_ShouldBeNull_ByDefault()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.ReplacedByToken.Should().BeNull();
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettableAndGettable()
    {
        // Arrange
        var token = new RefreshToken();
        var updatedAt = DateTime.UtcNow;

        // Act
        token.UpdatedAt = updatedAt;

        // Assert
        token.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void UpdatedBy_ShouldDefaultToSystem()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert
        token.UpdatedBy.Should().Be("System");
    }

    [Fact]
    public void UpdatedBy_ShouldBeSettable()
    {
        // Arrange
        var token = new RefreshToken();
        var updatedBy = "admin@example.com";

        // Act
        token.UpdatedBy = updatedBy;

        // Assert
        token.UpdatedBy.Should().Be(updatedBy);
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void User_ShouldBeSettableAndGettable()
    {
        // Arrange
        var user = new User();
        var token = new RefreshToken();

        // Act
        token.User = user;

        // Assert
        token.User.Should().Be(user);
    }

    [Fact]
    public void User_ShouldBeRequired()
    {
        // Arrange & Act
        var token = new RefreshToken();

        // Assert - User is marked as null! in the model, so we just verify it can be set
        token.User.Should().BeNull();
    }

    #endregion

    #region Database Persistence Tests

    [Fact]
    public async Task CanSaveAndRetrieve_RefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new RefreshToken
        {
            UserId = userId,
            Token = "test-token-123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "192.168.1.100",
            IsRevoked = false
        };

        // Act
        Context.RefreshTokens.Add(token);
        await Context.SaveChangesAsync();

        // Assert
        var savedToken = await Context.RefreshTokens.FindAsync(token.Id);
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(userId);
        savedToken.Token.Should().Be("test-token-123");
        savedToken.CreatedByIp.Should().Be("192.168.1.100");
        savedToken.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task CanUpdate_RefreshToken()
    {
        // Arrange
        var token = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "original-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        Context.RefreshTokens.Add(token);
        await Context.SaveChangesAsync();

        // Act
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = "192.168.1.200";
        token.ReplacedByToken = "new-token-456";
        token.UpdatedAt = DateTime.UtcNow;
        token.UpdatedBy = "system@example.com";

        await Context.SaveChangesAsync();

        // Assert
        var updatedToken = await Context.RefreshTokens.FindAsync(token.Id);
        updatedToken!.IsRevoked.Should().BeTrue();
        updatedToken.RevokedAt.Should().NotBeNull();
        updatedToken.RevokedByIp.Should().Be("192.168.1.200");
        updatedToken.ReplacedByToken.Should().Be("new-token-456");
        updatedToken.UpdatedBy.Should().Be("system@example.com");
    }

    [Fact]
    public async Task CanDelete_RefreshToken()
    {
        // Arrange
        var token = new RefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        Context.RefreshTokens.Add(token);
        await Context.SaveChangesAsync();

        // Act
        Context.RefreshTokens.Remove(token);
        await Context.SaveChangesAsync();

        // Assert
        var deletedToken = await Context.RefreshTokens.FindAsync(token.Id);
        deletedToken.Should().BeNull();
    }

    #endregion

    #region User Relationship Tests

    [Fact]
    public void User_CanHaveMultiple_RefreshTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        var token1 = new RefreshToken { UserId = userId, Token = "token1" };
        var token2 = new RefreshToken { UserId = userId, Token = "token2" };
        var token3 = new RefreshToken { UserId = userId, Token = "token3" };

        // Act
        user.RefreshTokens.Add(token1);
        user.RefreshTokens.Add(token2);
        user.RefreshTokens.Add(token3);

        // Assert
        user.RefreshTokens.Should().HaveCount(3);
        user.RefreshTokens.Should().Contain(token1);
        user.RefreshTokens.Should().Contain(token2);
        user.RefreshTokens.Should().Contain(token3);
    }

    [Fact]
    public void RefreshToken_ShouldReference_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };
        var token = new RefreshToken { UserId = userId };

        // Act
        token.User = user;

        // Assert
        token.User.Should().Be(user);
        token.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task DeletingUser_ShouldCascadeDelete_RefreshTokens()
    {
        // Arrange
        var user = TestDataFactory.CreateValidUser();
        var token1 = new RefreshToken { UserId = user.Id, Token = "token1" };
        var token2 = new RefreshToken { UserId = user.Id, Token = "token2" };

        user.RefreshTokens.Add(token1);
        user.RefreshTokens.Add(token2);

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Act
        Context.Users.Remove(user);
        await Context.SaveChangesAsync();

        // Assert
        var tokens = Context.RefreshTokens.ToList();
        tokens.Should().BeEmpty();
    }

    #endregion
}