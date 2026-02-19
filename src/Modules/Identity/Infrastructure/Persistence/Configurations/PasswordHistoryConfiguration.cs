using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;


namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Configurations;

public class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("password_histories", "identity");

        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ph => ph.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ph => ph.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ph => ph.ChangedByIp)
                 .HasColumnName("changed_by_ip")
                 .HasMaxLength(45);


        builder.Property(ph => ph.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ph => ph.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        builder.Property(ph => ph.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ph => ph.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        builder.HasIndex(ph => ph.UserId)
            .HasDatabaseName("ix_password_history_user_id");

        builder.HasIndex(ph => new { ph.UserId, ph.CreatedAt })
            .HasDatabaseName("ix_password_history_user_changed")
            .IsDescending(false, true);

        // Relationship
        builder.HasOne(ph => ph.User)
            .WithMany(u => u.PasswordHistories)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_password_history_users");
    }
}