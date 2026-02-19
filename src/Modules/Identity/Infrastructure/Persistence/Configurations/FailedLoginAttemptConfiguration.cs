using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;

namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Configurations;

public class FailedLoginAttemptConfiguration : IEntityTypeConfiguration<FailedLoginAttempt>
{
    public void Configure(EntityTypeBuilder<FailedLoginAttempt> builder)
    {
        builder.ToTable("failed_login_attempts", "identity");

        builder.HasKey(fla => fla.Id);

        builder.Property(fla => fla.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(fla => fla.UserId)
            .HasColumnName("user_id");

        builder.Property(fla => fla.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(fla => fla.IpAddress)
            .HasColumnName("ip_address")
            .IsRequired()
            .HasMaxLength(45);

        builder.Property(fla => fla.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(fla => fla.Reason)
            .HasColumnName("reason")
            .HasMaxLength(100);

        builder.Property(fla => fla.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(fla => fla.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        builder.Property(fla => fla.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(fla => fla.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        // Indexes
        builder.HasIndex(fla => fla.IpAddress)
            .HasDatabaseName("ix_failed_login_attempts_ip");

        builder.HasIndex(fla => fla.Email)
            .HasDatabaseName("ix_failed_login_attempts_email");

        builder.HasIndex(fla => fla.UserId)
            .HasDatabaseName("ix_failed_login_attempts_user_id");

        builder.HasIndex(fla => fla.CreatedAt)
            .HasDatabaseName("ix_failed_login_attempts_created_at");

        builder.HasIndex(fla => new { fla.IpAddress, fla.CreatedAt })
            .HasDatabaseName("ix_failed_login_attempts_ip_time")
            .IsDescending(false, true);

        // Relationship
        builder.HasOne(fla => fla.User)
            .WithMany(u => u.FailedLoginAttemptsLog)
            .HasForeignKey(fla => fla.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_failed_login_attempts_users");
    }
}