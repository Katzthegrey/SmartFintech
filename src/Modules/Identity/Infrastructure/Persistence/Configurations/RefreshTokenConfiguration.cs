using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;

namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // 1. Table configuration
        builder.ToTable("refresh_tokens", "identity");  

        // 2. Primary key
        builder.HasKey(rt => rt.Id);

        // 3. Properties
        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(rt => rt.IsRevoked) 
            .HasColumnName("is_revoked")
            .HasDefaultValue(false);

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(45);

        builder.Property(rt => rt.ReplacedByToken)
            .HasColumnName("replaced_by_token")
            .HasMaxLength(500);

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(45);

        // 4. IAuditableEntity properties 
        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rt => rt.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rt => rt.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        // 5. Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("ix_refresh_tokens_user_id");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("ix_refresh_tokens_expires_at");

    
        builder.HasCheckConstraint("ck_refresh_tokens_expiry",
            "expires_at > created_at");

        
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refresh_tokens_users");
    }
}