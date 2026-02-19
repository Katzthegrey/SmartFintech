using System;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByIp { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "System";
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "System";

    // Parameterless constructor for EF Core
    public RefreshToken() { }

    // Domain constructor for creating new tokens
    public RefreshToken(Guid userId, string token, DateTime expiresAt, string createdByIp)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp ?? throw new ArgumentNullException(nameof(createdByIp));
        CreatedAt = DateTime.UtcNow;
        CreatedBy = "System";
        UpdatedBy = "System";
        IsRevoked = false;
    }

    // Domain methods
    public void Revoke(string revokedByIp, string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked");

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp ?? throw new ArgumentNullException(nameof(revokedByIp));
        ReplacedByToken = replacedByToken;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsActive => !IsRevoked && !IsExpired;
}
