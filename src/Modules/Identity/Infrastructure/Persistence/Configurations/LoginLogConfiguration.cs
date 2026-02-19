using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;

namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Configurations;

public class LoginLogConfiguration : IEntityTypeConfiguration<LoginLog>
{
    public void Configure(EntityTypeBuilder<LoginLog> builder)
    {
        builder.ToTable("login_logs", "identity");

        builder.HasKey(ll => ll.Id);

        builder.Property(ll => ll.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ll => ll.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ll => ll.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(ll => ll.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(ll => ll.IsSuccess)
            .HasColumnName("is_success")
            .HasDefaultValue(false);

        builder.Property(ll => ll.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(100);

        builder.Property(ll => ll.TwoFactorUsed)
            .HasColumnName("two_factor_used")
            .HasDefaultValue(false);

        builder.Property(ll => ll.Location)
            .HasColumnName("location")
            .HasMaxLength(255);

        builder.Property(ll => ll.DeviceType)
            .HasColumnName("device_type")
            .HasMaxLength(50);

        builder.Property(ll => ll.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ll => ll.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        builder.Property(ll => ll.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ll => ll.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        // Indexes
        builder.HasIndex(ll => ll.UserId)
            .HasDatabaseName("ix_login_logs_user_id");

        builder.HasIndex(ll => ll.CreatedAt)
            .HasDatabaseName("ix_login_logs_created_at")
            .IsDescending(true);

        builder.HasIndex(ll => ll.IsSuccess)
            .HasDatabaseName("ix_login_logs_is_success");

        builder.HasIndex(ll => new { ll.UserId, ll.CreatedAt })
            .HasDatabaseName("ix_login_logs_user_time")
            .IsDescending(false, true);

        // Relationship
        builder.HasOne(ll => ll.User)
            .WithMany(u => u.LoginLogs)
            .HasForeignKey(ll => ll.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_login_logs_users");
    }
}