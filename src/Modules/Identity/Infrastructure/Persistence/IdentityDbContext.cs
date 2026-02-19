using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFintechFinancial.Modules.Identity.Domain.Constants;
using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using SmartFintechFinancial.Shared.Infrastructure.Persistence;

namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext
{
    private readonly ILogger<IdentityDbContext> _logger;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ILogger<IdentityDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    // ===== EXISTING DbSets =====
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<FailedLoginAttempt> FailedLoginAttempts => Set<FailedLoginAttempt>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();

    // ===== NEW: ROLE-BASED DbSets =====
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Validate GUIDs on model creation
        SystemGuids.ValidateGuids();
        _logger.LogInformation("System GUIDs validation passed");

        // ========== USER CONFIGURATION ==========
        ConfigureUser(modelBuilder);

        // ========== ROLE CONFIGURATION ==========
        ConfigureRole(modelBuilder);

        // ========== USER ROLE CONFIGURATION ==========
        ConfigureUserRole(modelBuilder);

        // ========== PERMISSION CONFIGURATION ==========
        ConfigurePermission(modelBuilder);

        // ========== ROLE PERMISSION CONFIGURATION ==========
        ConfigureRolePermission(modelBuilder);

        // ========== SEED DATA ==========
        SeedRoleData(modelBuilder);
        SeedPermissionData(modelBuilder);
        SeedRolePermissionData(modelBuilder);

        // ========== EXISTING CONFIGURATIONS ==========
        ConfigureRefreshToken(modelBuilder);
        ConfigurePasswordHistory(modelBuilder);
        ConfigureFailedLoginAttempt(modelBuilder);
        ConfigureLoginLog(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Table configuration
            entity.ToTable("users", "identity");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            // ===== INDEXES =====
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("ix_users_email");

            entity.HasIndex(e => e.KycStatus)
                  .HasDatabaseName("ix_users_kyc_status");

            entity.HasIndex(e => e.RiskLevel)
                  .HasDatabaseName("ix_users_risk_level");

            entity.HasIndex(e => e.SubscriptionTier)
                  .HasDatabaseName("ix_users_subscription_tier");

            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("ix_users_is_active");

            entity.HasIndex(e => e.EmailVerified)
                  .HasDatabaseName("ix_users_email_verified");

            entity.HasIndex(e => e.Phone)
                  .IsUnique()
                  .HasDatabaseName("ix_users_phone")
                  .HasFilter("phone IS NOT NULL");

            entity.HasIndex(e => e.IsFlaggedForReview)
                  .HasDatabaseName("ix_users_flagged_for_review");

            entity.HasIndex(e => e.AssignedFinancialAdvisor)
                  .HasDatabaseName("ix_users_assigned_advisor")
                  .HasFilter("assigned_financial_advisor IS NOT NULL");

            // ===== COMPOSITE INDEXES =====
            entity.HasIndex(e => new { e.IsActive, e.EmailVerified, e.KycStatus })
                  .HasDatabaseName("ix_users_status_composite");

            entity.HasIndex(e => new { e.RiskLevel, e.IsFlaggedForReview, e.KycStatus })
                  .HasDatabaseName("ix_users_risk_composite");

            entity.HasIndex(e => new { e.SubscriptionTier, e.RiskLevel })
                  .HasDatabaseName("ix_users_tier_risk");

            entity.HasIndex(e => new { e.CreatedAt, e.KycStatus })
                  .HasDatabaseName("ix_users_created_kyc");

            entity.HasIndex(e => new { e.LastLoginAt, e.IsActive })
                  .HasDatabaseName("ix_users_lastlogin_active")
                  .HasFilter("last_login_at IS NOT NULL");

            // ===== PROPERTIES (BASIC) =====
            entity.Property(e => e.Email)
                  .HasColumnName("email")
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.PasswordHash)
                  .HasColumnName("password_hash")
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.Phone)
                  .HasColumnName("phone")
                  .HasMaxLength(20);

            entity.Property(e => e.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

            entity.Property(e => e.EmailVerified)
                  .HasColumnName("email_verified")
                  .HasDefaultValue(false);

            // ===== SECURITY =====
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
                  .HasColumnName("two_factor_secret");

            entity.Property(e => e.TwoFactorEnabled)
                  .HasColumnName("two_factor_enabled")
                  .HasDefaultValue(false);

            entity.Property(e => e.BackupCodes)
                  .HasColumnName("backup_codes");

            entity.Property(e => e.LastLoginAt)
                  .HasColumnName("last_login_at");

            entity.Property(e => e.LastLoginIp)
                  .HasColumnName("last_login_ip")
                  .HasMaxLength(45);

            entity.Property(e => e.LastLoginUserAgent)
                  .HasColumnName("last_login_user_agent")
                  .HasMaxLength(500);

            // ===== KYC/AML =====
            entity.Property(e => e.FirstName)
                  .HasColumnName("first_name")
                  .HasMaxLength(100);

            entity.Property(e => e.LastName)
                  .HasColumnName("last_name")
                  .HasMaxLength(100);

            entity.Property(e => e.DateOfBirth)
                  .HasColumnName("date_of_birth");

            entity.Property(e => e.Address)
                  .HasColumnName("address")
                  .HasMaxLength(200);

            entity.Property(e => e.City)
                  .HasColumnName("city")
                  .HasMaxLength(50);

            entity.Property(e => e.PostalCode)
                  .HasColumnName("postal_code")
                  .HasMaxLength(10);

            entity.Property(e => e.Country)
                  .HasColumnName("country")
                  .HasMaxLength(50);

            entity.Property(e => e.AnnualIncome)
                  .HasColumnName("annual_income")
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.EmploymentStatus)
                  .HasColumnName("employment_status")
                  .HasMaxLength(50);

            entity.Property(e => e.SourceOfFunds)
                  .HasColumnName("source_of_funds")
                  .HasMaxLength(50);

            entity.Property(e => e.TaxIdNumber)
                  .HasColumnName("tax_id_number")
                  .HasMaxLength(100);

            entity.Property(e => e.KycStatus)
                  .HasColumnName("kyc_status")
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(KycStatus.Pending);

            entity.Property(e => e.KycVerifiedAt)
                  .HasColumnName("kyc_verified_at");

            entity.Property(e => e.KycVerifiedBy)
                  .HasColumnName("kyc_verified_by")
                  .HasMaxLength(100);

            entity.Property(e => e.KycRejectionReason)
                  .HasColumnName("kyc_rejection_reason");

            // ===== FRAUD & RISK =====
            entity.Property(e => e.RiskLevel)
                  .HasColumnName("risk_level")
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(RiskLevel.Low);

            entity.Property(e => e.RiskAssessedAt)
                  .HasColumnName("risk_assessed_at");

            entity.Property(e => e.RiskAssessedBy)
                  .HasColumnName("risk_assessed_by")
                  .HasMaxLength(100);

            entity.Property(e => e.RiskNotes)
                  .HasColumnName("risk_notes");

            entity.Property(e => e.DailyTransactionLimit)
                  .HasColumnName("daily_transaction_limit")
                  .HasColumnType("decimal(18,2)")
                  .HasDefaultValue(10000.00m);

            entity.Property(e => e.MonthlyTransactionLimit)
                  .HasColumnName("monthly_transaction_limit")
                  .HasColumnType("decimal(18,2)")
                  .HasDefaultValue(50000.00m);

            entity.Property(e => e.IsFlaggedForReview)
                  .HasColumnName("is_flagged_for_review")
                  .HasDefaultValue(false);

            entity.Property(e => e.FlaggedAt)
                  .HasColumnName("flagged_at");

            entity.Property(e => e.FlagReason)
                  .HasColumnName("flag_reason")
                  .HasMaxLength(500);

            // ===== PREFERENCES =====
            entity.Property(e => e.Timezone)
                  .HasColumnName("timezone")
                  .HasMaxLength(50)
                  .HasDefaultValue("UTC");

            entity.Property(e => e.Currency)
                  .HasColumnName("currency")
                  .HasMaxLength(3)
                  .HasDefaultValue("ZAR");

            entity.Property(e => e.Language)
                  .HasColumnName("language")
                  .HasMaxLength(5)
                  .HasDefaultValue("en");

            entity.Property(e => e.SubscriptionTier)
                  .HasColumnName("subscription_tier")
                  .HasMaxLength(50)
                  .HasDefaultValue("Free");

            entity.Property(e => e.InvestmentRiskTolerance)
                  .HasColumnName("investment_risk_tolerance")
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(RiskTolerance.Moderate);

            entity.Property(e => e.PrimaryInvestmentGoal)
                  .HasColumnName("primary_investment_goal")
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(InvestmentGoal.Growth);

            entity.Property(e => e.PreferredInvestmentTypes)
                  .HasColumnName("preferred_investment_types")
                  .HasMaxLength(200);

            // ===== GDPR/CONSENT =====
            entity.Property(e => e.ConsentGiven)
                  .HasColumnName("consent_given")
                  .HasDefaultValue(false);

            entity.Property(e => e.ConsentGivenAt)
                  .HasColumnName("consent_given_at");

            entity.Property(e => e.MarketingOptIn)
                  .HasColumnName("marketing_opt_in")
                  .HasDefaultValue(false);

            entity.Property(e => e.ConsentPreferences)
                  .HasColumnName("consent_preferences")
                  .HasMaxLength(1000);

            // ===== SUPPORT & COMPLIANCE =====
            entity.Property(e => e.AssignedFinancialAdvisor)
                  .HasColumnName("assigned_financial_advisor")
                  .HasMaxLength(50);

            entity.Property(e => e.AssignedComplianceOfficer)
                  .HasColumnName("assigned_compliance_officer")
                  .HasMaxLength(50);

            entity.Property(e => e.RequiresPeriodicReview)
                  .HasColumnName("requires_periodic_review")
                  .HasDefaultValue(false);

            entity.Property(e => e.NextReviewDate)
                  .HasColumnName("next_review_date");

            // ===== AUDIT PROPERTIES =====
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .ValueGeneratedOnAddOrUpdate()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedBy)
                  .HasColumnName("updated_by")
                  .HasMaxLength(100);

            // ===== RELATIONSHIPS =====
            entity.HasMany(e => e.UserRoles)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_user_roles_users");

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

            entity.HasMany(e => e.LoginLogs)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_login_logs_users");

            entity.HasMany(e => e.FailedLoginAttemptsLog)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_failed_login_attempts_users");

            // ===== QUERY FILTER =====
            entity.HasQueryFilter(e => e.IsActive);

            entity.HasComment("Users table with enhanced security, KYC/AML, and role-based access control");
        });
    }

    private void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles", "identity");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("ix_roles_name");

            entity.HasIndex(e => e.Category)
                  .HasDatabaseName("ix_roles_category");

            entity.HasIndex(e => e.IsSystemRole)
                  .HasDatabaseName("ix_roles_is_system_role");

            entity.HasIndex(e => e.Priority)
                  .HasDatabaseName("ix_roles_priority");

            entity.Property(e => e.Name)
                  .HasColumnName("name")
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.Description)
                  .HasColumnName("description")
                  .HasMaxLength(255);

            entity.Property(e => e.Category)
                  .HasColumnName("category")
                  .HasMaxLength(50)
                  .HasDefaultValue("General");

            entity.Property(e => e.IsSystemRole)
                  .HasColumnName("is_system_role")
                  .HasDefaultValue(false);

            entity.Property(e => e.CanBeAssigned)
                  .HasColumnName("can_be_assigned")
                  .HasDefaultValue(true);

            entity.Property(e => e.Priority)
                  .HasColumnName("priority")
                  .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .ValueGeneratedOnAddOrUpdate()
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.UserRoles)
                  .WithOne(e => e.Role)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_user_roles_roles");

            entity.HasMany(e => e.RolePermissions)
                  .WithOne(e => e.Role)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_role_permissions_roles");

            entity.HasComment("System roles for role-based authorization");
        });
    }

    private void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles", "identity");
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.RoleId)
                  .HasColumnName("role_id");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_user_roles_user_id");

            entity.HasIndex(e => e.RoleId)
                  .HasDatabaseName("ix_user_roles_role_id");

            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("ix_user_roles_is_active");

            entity.HasIndex(e => new { e.UserId, e.IsActive })
                  .HasDatabaseName("ix_user_roles_user_active");

            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("ix_user_roles_expires_at")
                  .HasFilter("expires_at IS NOT NULL");

            entity.Property(e => e.AssignedAt)
                  .HasColumnName("assigned_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.AssignedBy)
                  .HasColumnName("assigned_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.ExpiresAt)
                  .HasColumnName("expires_at");

            entity.Property(e => e.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

            entity.HasOne(e => e.User)
                  .WithMany(e => e.UserRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_user_roles_users");

            entity.HasOne(e => e.Role)
                  .WithMany(e => e.UserRoles)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_user_roles_roles");

            entity.HasCheckConstraint("ck_user_roles_expires_at",
                "expires_at IS NULL OR expires_at > assigned_at");

            entity.HasComment("Many-to-many relationship between users and roles");
        });
    }

    private void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions", "identity");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("ix_permissions_name");

            entity.HasIndex(e => e.Category)
                  .HasDatabaseName("ix_permissions_category");

            entity.HasIndex(e => e.Scope)
                  .HasDatabaseName("ix_permissions_scope");

            entity.HasIndex(e => new { e.Category, e.Scope })
                  .HasDatabaseName("ix_permissions_category_scope");

            entity.Property(e => e.Name)
                  .HasColumnName("name")
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.Description)
                  .HasColumnName("description")
                  .HasMaxLength(255);

            entity.Property(e => e.Category)
                  .HasColumnName("category")
                  .HasMaxLength(50);

            entity.Property(e => e.Scope)
                  .HasColumnName("scope")
                  .HasMaxLength(20)
                  .HasDefaultValue("system");

            entity.Property(e => e.IsSensitive)
                  .HasColumnName("is_sensitive")
                  .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.RolePermissions)
                  .WithOne(e => e.Permission)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_role_permissions_permissions");

            entity.HasComment("System permissions for fine-grained access control");
        });
    }

    private void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions", "identity");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.Property(e => e.RoleId)
                  .HasColumnName("role_id");

            entity.Property(e => e.PermissionId)
                  .HasColumnName("permission_id");

            entity.HasIndex(e => e.RoleId)
                  .HasDatabaseName("ix_role_permissions_role_id");

            entity.HasIndex(e => e.PermissionId)
                  .HasDatabaseName("ix_role_permissions_permission_id");

            entity.HasIndex(e => new { e.RoleId, e.CanDelegate })
                  .HasDatabaseName("ix_role_permissions_delegate");

            entity.Property(e => e.GrantedAt)
                  .HasColumnName("granted_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.GrantedBy)
                  .HasColumnName("granted_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.CanDelegate)
                  .HasColumnName("can_delegate")
                  .HasDefaultValue(false);

            entity.HasOne(e => e.Role)
                  .WithMany(e => e.RolePermissions)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_role_permissions_roles");

            entity.HasOne(e => e.Permission)
                  .WithMany(e => e.RolePermissions)
                  .HasForeignKey(e => e.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_role_permissions_permissions");

            entity.HasComment("Many-to-many relationship between roles and permissions");
        });
    }

    // ========== SEED DATA METHODS ==========
    private void SeedRoleData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var roles = new[]
        {
            new Role
            {
                Id = Guid.Parse(SystemGuids.Client),
                Name = RoleConstants.Client,
                Description = "Basic financial planning client",
                Category = "Client",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 10,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.Investor),
                Name = RoleConstants.Investor,
                Description = "Investment account holder",
                Category = "Client",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 20,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.PremiumInvestor),
                Name = RoleConstants.PremiumInvestor,
                Description = "High-value premium investor",
                Category = "Client",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 30,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.BusinessInvestor),
                Name = RoleConstants.BusinessInvestor,
                Description = "Business/corporate investor",
                Category = "Client",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 40,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.FinancialAdvisor),
                Name = RoleConstants.FinancialAdvisor,
                Description = "Registered financial advisor",
                Category = "Advisor",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 60,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.WealthManager),
                Name = RoleConstants.WealthManager,
                Description = "Portfolio and wealth manager",
                Category = "Management",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 70,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.SupportAgent),
                Name = RoleConstants.SupportAgent,
                Description = "Customer support agent",
                Category = "Support",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 50,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.FraudAnalyst),
                Name = RoleConstants.FraudAnalyst,
                Description = "Fraud detection and prevention specialist",
                Category = "Security",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 75,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.ComplianceOfficer),
                Name = RoleConstants.ComplianceOfficer,
                Description = "Regulatory compliance officer",
                Category = "Compliance",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 90,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.FinanceAdmin),
                Name = RoleConstants.FinanceAdmin,
                Description = "Financial operations administrator",
                Category = "Admin",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 80,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Role
            {
                Id = Guid.Parse(SystemGuids.SuperAdmin),
                Name = RoleConstants.SuperAdmin,
                Description = "System administrator with full access",
                Category = "Admin",
                IsSystemRole = true,
                CanBeAssigned = true,
                Priority = 100,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        };

        modelBuilder.Entity<Role>().HasData(roles);
        _logger.LogInformation("Configured {Count} system roles with fixed GUIDs", roles.Length);
    }

    private void SeedPermissionData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var permissions = new[]
        {
            // Account Permissions
            new Permission
            {
                Id = Guid.Parse(SystemGuids.AccountsReadSelf),
                Name = "accounts:read:self",
                Description = "View own accounts",
                Category = "Account",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.AccountsReadAll),
                Name = "accounts:read:all",
                Description = "View all accounts",
                Category = "Account",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.AccountsCreate),
                Name = "accounts:create",
                Description = "Create new accounts",
                Category = "Account",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.AccountsUpdateSelf),
                Name = "accounts:update:self",
                Description = "Update own account",
                Category = "Account",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.AccountsUpdateAll),
                Name = "accounts:update:all",
                Description = "Update any account",
                Category = "Account",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            
            // Transaction Permissions
            new Permission
            {
                Id = Guid.Parse(SystemGuids.TransactionsReadSelf),
                Name = "transactions:read:self",
                Description = "View own transactions",
                Category = "Transaction",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.TransactionsReadAll),
                Name = "transactions:read:all",
                Description = "View all transactions",
                Category = "Transaction",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.TransactionsCreate),
                Name = "transactions:create",
                Description = "Create transactions",
                Category = "Transaction",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.TransactionsReverse),
                Name = "transactions:reverse",
                Description = "Reverse transactions",
                Category = "Transaction",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            
            // Investment Permissions
            new Permission
            {
                Id = Guid.Parse(SystemGuids.InvestmentsReadSelf),
                Name = "investments:read:self",
                Description = "View own investments",
                Category = "Investment",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.InvestmentsReadAll),
                Name = "investments:read:all",
                Description = "View all investments",
                Category = "Investment",
                Scope = "advisor",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.InvestmentsRecommend),
                Name = "investments:recommend",
                Description = "Recommend investments",
                Category = "Investment",
                Scope = "advisor",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.InvestmentsManagePortfolio),
                Name = "investments:manage:portfolio",
                Description = "Manage investment portfolios",
                Category = "Investment",
                Scope = "advisor",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            
            // Fraud Detection Permissions
            new Permission
            {
                Id = Guid.Parse(SystemGuids.FraudAlertsRead),
                Name = "fraud:alerts:read",
                Description = "View fraud alerts",
                Category = "Fraud",
                Scope = "security",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.FraudTransactionsReview),
                Name = "fraud:transactions:review",
                Description = "Review suspicious transactions",
                Category = "Fraud",
                Scope = "security",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.FraudRulesManage),
                Name = "fraud:rules:manage",
                Description = "Manage fraud detection rules",
                Category = "Fraud",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            
            // Compliance Permissions
            new Permission
            {
                Id = Guid.Parse(SystemGuids.ComplianceReportsGenerate),
                Name = "compliance:reports:generate",
                Description = "Generate compliance reports",
                Category = "Compliance",
                Scope = "compliance",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.ComplianceAuditRead),
                Name = "compliance:audit:read",
                Description = "View audit logs",
                Category = "Compliance",
                Scope = "compliance",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            
            // User Management Permissions
            new Permission
            {
                Id = Guid.Parse(SystemGuids.UsersReadSelf),
                Name = "users:read:self",
                Description = "View own profile",
                Category = "User",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.UsersReadAll),
                Name = "users:read:all",
                Description = "View all user profiles",
                Category = "User",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.UsersUpdateSelf),
                Name = "users:update:self",
                Description = "Update own profile",
                Category = "User",
                Scope = "client",
                IsSensitive = false,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.UsersUpdateAll),
                Name = "users:update:all",
                Description = "Update any user profile",
                Category = "User",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            },
            new Permission
            {
                Id = Guid.Parse(SystemGuids.UsersRolesManage),
                Name = "users:roles:manage",
                Description = "Manage user roles",
                Category = "User",
                Scope = "admin",
                IsSensitive = true,
                CreatedAt = seedDate
            }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);
        _logger.LogInformation("Configured {Count} system permissions with fixed GUIDs", permissions.Length);
    }

    private void SeedRolePermissionData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rolePermissions = new List<RolePermission>();

        void AddRolePermission(string roleGuid, string permissionGuid, bool canDelegate = false)
        {
            rolePermissions.Add(new RolePermission
            {
                RoleId = Guid.Parse(roleGuid),
                PermissionId = Guid.Parse(permissionGuid),
                GrantedBy = "system:seed",
                GrantedAt = seedDate,
                CanDelegate = canDelegate
            });
        }

        // ===== CLIENT PERMISSIONS =====
        AddRolePermission(SystemGuids.Client, SystemGuids.AccountsReadSelf);
        AddRolePermission(SystemGuids.Client, SystemGuids.TransactionsReadSelf);
        AddRolePermission(SystemGuids.Client, SystemGuids.TransactionsCreate);
        AddRolePermission(SystemGuids.Client, SystemGuids.InvestmentsReadSelf);
        AddRolePermission(SystemGuids.Client, SystemGuids.UsersReadSelf);
        AddRolePermission(SystemGuids.Client, SystemGuids.UsersUpdateSelf);
        AddRolePermission(SystemGuids.Client, SystemGuids.AccountsUpdateSelf);

        // ===== INVESTOR PERMISSIONS =====
        AddRolePermission(SystemGuids.Investor, SystemGuids.AccountsReadSelf);
        AddRolePermission(SystemGuids.Investor, SystemGuids.TransactionsReadSelf);
        AddRolePermission(SystemGuids.Investor, SystemGuids.TransactionsCreate);
        AddRolePermission(SystemGuids.Investor, SystemGuids.InvestmentsReadSelf);
        AddRolePermission(SystemGuids.Investor, SystemGuids.UsersReadSelf);
        AddRolePermission(SystemGuids.Investor, SystemGuids.UsersUpdateSelf);
        AddRolePermission(SystemGuids.Investor, SystemGuids.AccountsUpdateSelf);

        // ===== PREMIUM INVESTOR PERMISSIONS =====
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.AccountsReadSelf);
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.TransactionsReadSelf);
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.TransactionsCreate);
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.InvestmentsReadSelf);
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.UsersReadSelf);
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.UsersUpdateSelf);
        AddRolePermission(SystemGuids.PremiumInvestor, SystemGuids.AccountsUpdateSelf);

        // ===== BUSINESS INVESTOR PERMISSIONS =====
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.AccountsReadSelf);
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.TransactionsReadSelf);
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.TransactionsCreate);
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.InvestmentsReadSelf);
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.UsersReadSelf);
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.UsersUpdateSelf);
        AddRolePermission(SystemGuids.BusinessInvestor, SystemGuids.AccountsUpdateSelf);

        // ===== FINANCIAL ADVISOR PERMISSIONS =====
        AddRolePermission(SystemGuids.FinancialAdvisor, SystemGuids.InvestmentsReadAll);
        AddRolePermission(SystemGuids.FinancialAdvisor, SystemGuids.InvestmentsRecommend);
        AddRolePermission(SystemGuids.FinancialAdvisor, SystemGuids.InvestmentsManagePortfolio);
        AddRolePermission(SystemGuids.FinancialAdvisor, SystemGuids.UsersReadAll);

        // ===== WEALTH MANAGER PERMISSIONS =====
        AddRolePermission(SystemGuids.WealthManager, SystemGuids.InvestmentsReadAll);
        AddRolePermission(SystemGuids.WealthManager, SystemGuids.InvestmentsRecommend);
        AddRolePermission(SystemGuids.WealthManager, SystemGuids.InvestmentsManagePortfolio);
        AddRolePermission(SystemGuids.WealthManager, SystemGuids.UsersReadAll);
        AddRolePermission(SystemGuids.WealthManager, SystemGuids.AccountsReadAll);
        AddRolePermission(SystemGuids.WealthManager, SystemGuids.TransactionsReadAll);

        // ===== SUPPORT AGENT PERMISSIONS =====
        AddRolePermission(SystemGuids.SupportAgent, SystemGuids.UsersReadAll);
        AddRolePermission(SystemGuids.SupportAgent, SystemGuids.UsersUpdateAll);

        // ===== FRAUD ANALYST PERMISSIONS =====
        AddRolePermission(SystemGuids.FraudAnalyst, SystemGuids.FraudAlertsRead);
        AddRolePermission(SystemGuids.FraudAnalyst, SystemGuids.FraudTransactionsReview);
        AddRolePermission(SystemGuids.FraudAnalyst, SystemGuids.TransactionsReadAll);
        AddRolePermission(SystemGuids.FraudAnalyst, SystemGuids.AccountsReadAll);
        AddRolePermission(SystemGuids.FraudAnalyst, SystemGuids.UsersReadAll);

        // ===== COMPLIANCE OFFICER PERMISSIONS =====
        AddRolePermission(SystemGuids.ComplianceOfficer, SystemGuids.ComplianceReportsGenerate);
        AddRolePermission(SystemGuids.ComplianceOfficer, SystemGuids.ComplianceAuditRead);
        AddRolePermission(SystemGuids.ComplianceOfficer, SystemGuids.TransactionsReadAll);
        AddRolePermission(SystemGuids.ComplianceOfficer, SystemGuids.AccountsReadAll);
        AddRolePermission(SystemGuids.ComplianceOfficer, SystemGuids.UsersReadAll);

        // ===== FINANCE ADMIN PERMISSIONS =====
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.AccountsReadAll);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.AccountsCreate);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.AccountsUpdateAll);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.TransactionsReadAll);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.TransactionsReverse);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.UsersReadAll);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.UsersUpdateAll);
        AddRolePermission(SystemGuids.FinanceAdmin, SystemGuids.UsersRolesManage);

        // ===== SUPER ADMIN PERMISSIONS =====
        var allPermissionGuids = new[]
        {
            SystemGuids.AccountsReadSelf, SystemGuids.AccountsReadAll, SystemGuids.AccountsCreate,
            SystemGuids.AccountsUpdateSelf, SystemGuids.AccountsUpdateAll,
            SystemGuids.TransactionsReadSelf, SystemGuids.TransactionsReadAll, SystemGuids.TransactionsCreate,
            SystemGuids.TransactionsReverse,
            SystemGuids.InvestmentsReadSelf, SystemGuids.InvestmentsReadAll, SystemGuids.InvestmentsRecommend,
            SystemGuids.InvestmentsManagePortfolio,
            SystemGuids.FraudAlertsRead, SystemGuids.FraudTransactionsReview, SystemGuids.FraudRulesManage,
            SystemGuids.ComplianceReportsGenerate, SystemGuids.ComplianceAuditRead,
            SystemGuids.UsersReadSelf, SystemGuids.UsersReadAll, SystemGuids.UsersUpdateSelf,
            SystemGuids.UsersUpdateAll, SystemGuids.UsersRolesManage
        };

        foreach (var permissionGuid in allPermissionGuids)
        {
            AddRolePermission(SystemGuids.SuperAdmin, permissionGuid, canDelegate: true);
        }

        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);
        _logger.LogInformation("Configured {Count} role-permission relationships", rolePermissions.Count);
    }

    // ========== UPDATED CONFIGURATION METHODS ==========
    private void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens", "identity");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

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

            entity.Property(e => e.IsRevoked)
                  .HasColumnName("is_revoked")
                  .HasDefaultValue(false);

            entity.Property(e => e.RevokedAt)
                  .HasColumnName("revoked_at");

            entity.Property(e => e.RevokedByIp)
                  .HasColumnName("revoked_by_ip")
                  .HasMaxLength(45);

            entity.Property(e => e.ReplacedByToken)
                  .HasColumnName("replaced_by_token")
                  .HasMaxLength(500);

            entity.Property(e => e.CreatedByIp)
                  .HasColumnName("created_by_ip")
                  .HasMaxLength(45);

            // IAuditableEntity properties
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedBy)
                  .HasColumnName("updated_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            // Indexes
            entity.HasIndex(e => e.Token)
                  .IsUnique()
                  .HasDatabaseName("ix_refresh_tokens_token");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_refresh_tokens_user_id");

            entity.HasIndex(e => e.ExpiresAt)
                  .HasDatabaseName("ix_refresh_tokens_expires_at");

            // Check constraint
            entity.HasCheckConstraint("ck_refresh_tokens_expiry",
                "expires_at > created_at");

            // Relationship
            entity.HasOne(e => e.User)
                  .WithMany(e => e.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_refresh_tokens_users");
        });
    }

    private void ConfigurePasswordHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("password_history", "identity");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();

            entity.Property(e => e.PasswordHash)
                  .HasColumnName("password_hash")
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.ChangedByIp)
                  .HasColumnName("changed_by_ip")
                  .HasMaxLength(45);

            // IAuditableEntity properties
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedBy)
                  .HasColumnName("updated_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            // Indexes
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_password_history_user_id");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt })
                  .HasDatabaseName("ix_password_history_user_changed")
                  .IsDescending(false, true);

            // Relationship
            entity.HasOne(e => e.User)
                  .WithMany(e => e.PasswordHistories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_password_histories_users");
        });
    }

    private void ConfigureFailedLoginAttempt(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FailedLoginAttempt>(entity =>
        {
            entity.ToTable("failed_login_attempts", "identity");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.Email)
                  .HasColumnName("email")
                  .HasMaxLength(255);

            entity.Property(e => e.IpAddress)
                  .HasColumnName("ip_address")
                  .IsRequired()
                  .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                  .HasColumnName("user_agent")
                  .HasMaxLength(500);

            entity.Property(e => e.Reason)
                  .HasColumnName("reason")
                  .HasMaxLength(100);

            entity.Property(e => e.AttemptNumber)
                  .HasColumnName("attempt_number")
                  .HasDefaultValue(1);

            // IAuditableEntity properties
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedBy)
                  .HasColumnName("updated_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            // Indexes
            entity.HasIndex(e => e.IpAddress)
                  .HasDatabaseName("ix_failed_login_attempts_ip");

            entity.HasIndex(e => e.Email)
                  .HasDatabaseName("ix_failed_login_attempts_email");

            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_failed_login_attempts_user_id");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("ix_failed_login_attempts_created_at");

            entity.HasIndex(e => new { e.IpAddress, e.CreatedAt })
                  .HasDatabaseName("ix_failed_login_attempts_ip_time")
                  .IsDescending(false, true);

            // Relationship
            entity.HasOne(e => e.User)
                  .WithMany(e => e.FailedLoginAttemptsLog)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_failed_login_attempts_users");
        });
    }

    private void ConfigureLoginLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.ToTable("login_logs", "identity");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id")
                  .IsRequired();

            entity.Property(e => e.IpAddress)
                  .HasColumnName("ip_address")
                  .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                  .HasColumnName("user_agent")
                  .HasMaxLength(500);

            entity.Property(e => e.IsSuccess)
                  .HasColumnName("is_success")
                  .HasDefaultValue(false);

            entity.Property(e => e.FailureReason)
                  .HasColumnName("failure_reason")
                  .HasMaxLength(100);

            entity.Property(e => e.TwoFactorUsed)
                  .HasColumnName("two_factor_used")
                  .HasDefaultValue(false);

            entity.Property(e => e.Location)
                  .HasColumnName("location")
                  .HasMaxLength(255);

            entity.Property(e => e.DeviceType)
                  .HasColumnName("device_type")
                  .HasMaxLength(50);

            // IAuditableEntity properties
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedBy)
                  .HasColumnName("updated_by")
                  .HasMaxLength(100)
                  .HasDefaultValue("system");

            // Indexes
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("ix_login_logs_user_id");

            entity.HasIndex(e => e.CreatedAt)
                  .HasDatabaseName("ix_login_logs_created_at")
                  .IsDescending(true);

            entity.HasIndex(e => e.IsSuccess)
                  .HasDatabaseName("ix_login_logs_is_success");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt })
                  .HasDatabaseName("ix_login_logs_user_time")
                  .IsDescending(false, true);

            // Relationship
            entity.HasOne(e => e.User)
                  .WithMany(e => e.LoginLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_login_logs_users");
        });
    }

    // ========== OVERRIDEN METHODS ==========
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        optionsBuilder.EnableSensitiveDataLogging(environment == "Development");
        optionsBuilder.EnableDetailedErrors(environment == "Development");

        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

                // For new entities, UpdatedAt should also be set
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = GetCurrentUser() ?? "system";
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = GetCurrentUser() ?? "system";
            }
        }
    }

    private string? GetCurrentUser()
    {
        // In a real implementation, this would get the current user from HttpContext
        // For example: _httpContextAccessor.HttpContext?.User?.Identity?.Name
        return "system";
    }
}