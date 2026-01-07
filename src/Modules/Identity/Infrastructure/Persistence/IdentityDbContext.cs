using Microsoft.EntityFrameworkCore;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Shared.Infrastructure.Persistence;

namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    // DbSet properties for ALL entities
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========== USER CONFIGURATION ==========
        modelBuilder.Entity<User>(entity =>
        {
            // Table name and schema
            entity.ToTable("users", "identity");

            // Primary key with PostgreSQL UUID generation
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            // ===== INDEXES =====
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("ix_users_email");

            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("ix_users_is_active");

            entity.HasIndex(e => e.KycStatus)
                  .HasDatabaseName("ix_users_kyc_status");

            entity.HasIndex(e => e.EmailVerified)
                  .HasDatabaseName("ix_users_email_verified");

            entity.HasIndex(e => e.Phone)
                  .HasDatabaseName("ix_users_phone")
                  .IsUnique()
                  .HasFilter("phone IS NOT NULL");

            entity.HasIndex(e => new { e.IsActive, e.EmailVerified, e.KycStatus })
                  .HasDatabaseName("ix_users_status_composite");

            // ===== BASIC PROPERTIES =====
            entity.Property(e => e.Email)
                  .HasColumnName("email")
                  .IsRequired()
                  .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                  .HasColumnName("password_hash")
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Phone)
                  .HasColumnName("phone")
                  .HasMaxLength(20);

            entity.Property(e => e.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

            entity.Property(e => e.EmailVerified)
                  .HasColumnName("email_verified")
                  .HasDefaultValue(false);

            // ===== SECURITY PROPERTIES =====
            entity.Property(e => e.FailedLoginAttempts)
                  .HasColumnName("failed_login_attempts")
                  .HasDefaultValue(0);

            entity.Property(e => e.LockedUntil)
                  .HasColumnName("locked_until");

            entity.Property(e => e.PasswordChangedAt)
                  .HasColumnName("password_changed_at");

            entity.Property(e => e.PasswordExpiresAt)
                  .HasColumnName("password_expires_at");

            entity.Property(e => e.TwoFactorSecret)
                  .HasColumnName("two_factor_secret")
                  .HasMaxLength(500);

            entity.Property(e => e.TwoFactorEnabled)
                  .HasColumnName("two_factor_enabled")
                  .HasDefaultValue(false);

            entity.Property(e => e.BackupCodes)
                  .HasColumnName("backup_codes");

            // ===== AUDIT & TRACKING =====
            entity.Property(e => e.LastLoginAt)
                  .HasColumnName("last_login_at");

            entity.Property(e => e.LastLoginIp)
                  .HasColumnName("last_login_ip")
                  .HasMaxLength(45);

            entity.Property(e => e.LastLoginUserAgent)
                  .HasColumnName("last_login_user_agent")
                  .HasMaxLength(500);

            // ===== KYC/AML PROPERTIES =====
            entity.Property(e => e.FirstName)
                  .HasColumnName("first_name")
                  .HasMaxLength(100);

            entity.Property(e => e.LastName)
                  .HasColumnName("last_name")
                  .HasMaxLength(100);

            entity.Property(e => e.DateOfBirth)
                  .HasColumnName("date_of_birth")
                  .HasColumnType("date");

            entity.Property(e => e.KycStatus)
                  .HasColumnName("kyc_status")
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(KycStatus.Pending);

            entity.Property(e => e.KycVerifiedAt)
                  .HasColumnName("kyc_verified_at");

            // ===== PREFERENCES =====
            entity.Property(e => e.Timezone)
                  .HasColumnName("timezone")
                  .HasMaxLength(50)
                  .HasDefaultValue("UTC");

            entity.Property(e => e.Currency)
                  .HasColumnName("currency")
                  .HasMaxLength(3)
                  .IsFixedLength()
                  .HasDefaultValue("R");

            entity.Property(e => e.Language)
                  .HasColumnName("language")
                  .HasMaxLength(10)
                  .HasDefaultValue("en");

            entity.Property(e => e.SubscriptionTier)
                  .HasColumnName("subscription_tier")
                  .HasMaxLength(50)
                  .HasDefaultValue("Free");

            // ===== GDPR/CONSENT =====
            entity.Property(e => e.ConsentGiven)
                  .HasColumnName("consent_given")
                  .HasDefaultValue(false);

            entity.Property(e => e.ConsentGivenAt)
                  .HasColumnName("consent_given_at");

            entity.Property(e => e.MarketingOptIn)
                  .HasColumnName("marketing_opt_in")
                  .HasDefaultValue(false);

            // ===== AUDIT TRAIL PROPERTIES =====
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .ValueGeneratedOnAddOrUpdate()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasComment("Automatically updated on save");

            entity.Property(e => e.UpdatedBy)
                  .HasColumnName("updated_by")
                  .HasMaxLength(100);

            // ===== RELATIONSHIPS =====
            entity.HasMany(e => e.RefreshTokens)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_refresh_tokens_users");

            entity.HasMany(e => e.PasswordHistories)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_password_histories_users");

            // ===== QUERY FILTER =====
            entity.HasQueryFilter(e => e.IsActive);

            // ===== TABLE COMMENT =====
            entity.HasComment("User accounts for authentication and identity management");
        });

        // ========== REFRESH TOKEN CONFIGURATION ==========
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens", "identity");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            // ===== INDEXES =====
            entity.HasIndex(e => e.Token)
                  .IsUnique()
                  .HasDatabaseName("ix_refresh_tokens_token");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_refresh_tokens_user_id");

            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("ix_refresh_tokens_expires_at");

            entity.HasIndex(e => new { e.UserId, e.ExpiresAt, e.RevokedAt })
                  .HasDatabaseName("ix_refresh_tokens_user_expiry_status");

            // ===== PROPERTIES =====
            entity.Property(e => e.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();

            entity.Property(e => e.Token)
                  .HasColumnName("token")
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.ExpiresAt)
                  .HasColumnName("expires_at")
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedByIp)
                  .HasColumnName("created_by_ip")
                  .HasMaxLength(45);

            entity.Property(e => e.RevokedAt)
                  .HasColumnName("revoked_at");

            entity.Property(e => e.RevokedByIp)
                  .HasColumnName("revoked_by_ip")
                  .HasMaxLength(45);

            entity.Property(e => e.ReplacedByToken)
                  .HasColumnName("replaced_by_token")
                  .HasMaxLength(500);

         

            // ===== FOREIGN KEY =====
            entity.HasOne(e => e.User)
                  .WithMany(e => e.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_refresh_tokens_users");

            // ===== QUERY FILTER =====
            entity.HasQueryFilter(e => e.RevokedAt == null && e.ExpiresAt > DateTime.UtcNow);

            // ===== TABLE COMMENT =====
            entity.HasComment("Refresh tokens for JWT authentication and session management");
        });

        // ========== PASSWORD HISTORY CONFIGURATION ==========
        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("password_histories", "identity");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            // ===== INDEXES =====
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_password_histories_user_id");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("ix_password_histories_created_at");

            entity.HasIndex(e => new { e.UserId, e.PasswordHash })
                  .IsUnique()
                  .HasDatabaseName("ix_password_histories_user_password")
                  .HasFilter(null);

            // ===== PROPERTIES =====
            entity.Property(e => e.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();

            entity.Property(e => e.PasswordHash)
                  .HasColumnName("password_hash")
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ===== FOREIGN KEY =====
            entity.HasOne(e => e.User)
                  .WithMany(e => e.PasswordHistories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_password_histories_users");

            // ===== CHECK CONSTRAINT (PostgreSQL) =====
            entity.HasCheckConstraint(
                "ck_password_histories_created_at",
                "created_at <= CURRENT_TIMESTAMP");

            // ===== TABLE COMMENT =====
            entity.HasComment("Historical password hashes for password reuse prevention");
        });

        // ========== GLOBAL CONFIGURATIONS ==========
        // Set naming convention
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Remove 'DbSet' prefix from table names if present
            var tableName = entity.GetTableName();
            if (tableName != null && tableName.EndsWith("DbSet"))
            {
                entity.SetTableName(tableName.Replace("DbSet", ""));
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback for development
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=SmartFintechFinancial;Username=postgres;Password=postgres",
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.GetName().Name);
                    npgsqlOptions.SetPostgresVersion(new Version(14, 0));
                });
        }

        // Enable sensitive data logging only in development
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        optionsBuilder.EnableSensitiveDataLogging(environment == "Development");
        optionsBuilder.EnableDetailedErrors(environment == "Development");

        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = GetCurrentUser() ?? "system";
            }

            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = GetCurrentUser() ?? "system";
            }
        }
    }

    private string? GetCurrentUser()
    {
        // TODO: Implement your user context access logic
        // This could come from IHttpContextAccessor, ICurrentUserService, etc.
        // For now, return system
        return "system";

        // Example with IHttpContextAccessor:
        // var httpContext = _httpContextAccessor.HttpContext;
        // return httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
    }
}