using SmartFintechFinancial.Shared.Infrastructure.Persistence;
using System;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities;

public class PasswordHistory : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    public string ChangedByIp { get; set; } = string.Empty;

    public DateTime? UpdatedAt {  get; set; }

    public string? UpdatedBy { get; set; }
}



