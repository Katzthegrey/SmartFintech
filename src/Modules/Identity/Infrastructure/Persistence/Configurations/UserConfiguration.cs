using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;


namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        // ===== ID & BASIC INFO =====
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .HasDefaultValue(false);

        // ===== SECURITY FIELDS =====
        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0);

        builder.Property(u => u.LockedUntil)
            .HasColumnName("locked_until");

        builder.Property(u => u.PasswordChangedAt)
            .HasColumnName("password_changed_at");

        builder.Property(u => u.PasswordExpiresAt)
            .HasColumnName("password_expires_at");

        builder.Property(u => u.TwoFactorSecret)
            .HasColumnName("two_factor_secret")
            .HasMaxLength(255);

        builder.Property(u => u.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled")
            .HasDefaultValue(false);

        builder.Property(u => u.BackupCodes)
            .HasColumnName("backup_codes");

        // ===== AUDIT & TRACKING =====
        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.LastLoginIp)
            .HasColumnName("last_login_ip")
            .HasMaxLength(45);

        builder.Property(u => u.LastLoginUserAgent)
            .HasColumnName("last_login_user_agent")
            .HasMaxLength(500);

        // ===== KYC/AML FIELDS =====
        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth");

        builder.Property(u => u.KycStatus)
            .HasColumnName("kyc_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(KycStatus.Pending);

        builder.Property(u => u.KycVerifiedAt)
            .HasColumnName("kyc_verified_at");

        // ===== PREFERENCES =====
        builder.Property(u => u.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        builder.Property(u => u.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("R");

        builder.Property(u => u.Language)
            .HasColumnName("language")
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(u => u.SubscriptionTier)
            .HasColumnName("subscription_tier")
            .HasMaxLength(50)
            .HasDefaultValue("Free");

        // ===== GDPR/CONSENT =====
        builder.Property(u => u.ConsentGiven)
            .HasColumnName("consent_given")
            .HasDefaultValue(false);

        builder.Property(u => u.ConsentGivenAt)
            .HasColumnName("consent_given_at");

        builder.Property(u => u.MarketingOptIn)
            .HasColumnName("marketing_opt_in")
            .HasDefaultValue(false);

        // ===== AUDIT TRAIL (IAuditableEntity) =====
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        // ===== CRITICAL: RELATIONSHIPS =====
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PasswordHistories)
            .WithOne(ph => ph.User)
            .HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== OPTIONAL: ADDITIONAL INDEXES =====
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("ix_users_is_active");

        builder.HasIndex(u => u.KycStatus)
            .HasDatabaseName("ix_users_kyc_status");

        builder.HasIndex(u => u.Phone)
            .HasDatabaseName("ix_users_phone")
            .IsUnique()
            .HasFilter("phone IS NOT NULL");

        // ===== QUERY FILTER =====
        builder.HasQueryFilter(u => u.IsActive);
    }
}