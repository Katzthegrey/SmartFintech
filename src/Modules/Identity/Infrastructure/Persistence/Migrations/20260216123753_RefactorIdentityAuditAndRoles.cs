using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartFintechFinancial.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorIdentityAuditAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_user_expiry_status",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_password_histories",
                schema: "identity",
                table: "password_histories");

            migrationBuilder.DropIndex(
                name: "ix_password_histories_created_at",
                schema: "identity",
                table: "password_histories");

            migrationBuilder.DropIndex(
                name: "ix_password_histories_user_password",
                schema: "identity",
                table: "password_histories");

            migrationBuilder.DropCheckConstraint(
                name: "ck_password_histories_created_at",
                schema: "identity",
                table: "password_histories");

            migrationBuilder.RenameTable(
                name: "password_histories",
                schema: "identity",
                newName: "password_history",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "ix_password_histories_user_id",
                schema: "identity",
                table: "password_history",
                newName: "ix_password_history_user_id");

            migrationBuilder.AlterTable(
                name: "users",
                schema: "identity",
                comment: "Users table with enhanced security, KYC/AML, and role-based access control",
                oldComment: "User accounts for authentication and identity management");

            migrationBuilder.AlterTable(
                name: "refresh_tokens",
                schema: "identity",
                oldComment: "Refresh tokens for JWT authentication and session management");

            migrationBuilder.AlterTable(
                name: "password_history",
                schema: "identity",
                oldComment: "Historical password hashes for password reuse prevention");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP",
                oldComment: "Automatically updated on save");

            migrationBuilder.AlterColumn<string>(
                name: "two_factor_secret",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "identity",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "language",
                schema: "identity",
                table: "users",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldDefaultValue: "en");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "identity",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "identity",
                table: "users",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ZAR",
                oldClrType: typeof(string),
                oldType: "character(3)",
                oldFixedLength: true,
                oldMaxLength: 3,
                oldDefaultValue: "R");

            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "identity",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "annual_income",
                schema: "identity",
                table: "users",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assigned_compliance_officer",
                schema: "identity",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assigned_financial_advisor",
                schema: "identity",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "identity",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "consent_preferences",
                schema: "identity",
                table: "users",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                schema: "identity",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_transaction_limit",
                schema: "identity",
                table: "users",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 10000.00m);

            migrationBuilder.AddColumn<string>(
                name: "employment_status",
                schema: "identity",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "flag_reason",
                schema: "identity",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "flagged_at",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "investment_risk_tolerance",
                schema: "identity",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Moderate");

            migrationBuilder.AddColumn<bool>(
                name: "is_flagged_for_review",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "kyc_rejection_reason",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "kyc_verified_by",
                schema: "identity",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_transaction_limit",
                schema: "identity",
                table: "users",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 50000.00m);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_review_date",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postal_code",
                schema: "identity",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_investment_types",
                schema: "identity",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "primary_investment_goal",
                schema: "identity",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Growth");

            migrationBuilder.AddColumn<bool>(
                name: "requires_periodic_review",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "risk_assessed_at",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "risk_assessed_by",
                schema: "identity",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "risk_level",
                schema: "identity",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Low");

            migrationBuilder.AddColumn<string>(
                name: "risk_notes",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_of_funds",
                schema: "identity",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tax_id_number",
                schema: "identity",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "identity",
                table: "refresh_tokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<bool>(
                name: "is_revoked",
                schema: "identity",
                table: "refresh_tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "identity",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                schema: "identity",
                table: "refresh_tokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "identity",
                table: "password_history",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "changed_by_ip",
                schema: "identity",
                table: "password_history",
                type: "character varying(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "identity",
                table: "password_history",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "identity",
                table: "password_history",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "updated_by",
                schema: "identity",
                table: "password_history",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "system");

            migrationBuilder.AddPrimaryKey(
                name: "PK_password_history",
                schema: "identity",
                table: "password_history",
                column: "id");

            migrationBuilder.CreateTable(
                name: "failed_login_attempts",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "system"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_failed_login_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_failed_login_attempts_users",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "login_logs",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_success = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    failure_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    two_factor_used = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "system"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "system")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_login_logs_users",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    scope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "system"),
                    is_sensitive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.id);
                },
                comment: "System permissions for fine-grained access control");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_be_assigned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                },
                comment: "System roles for role-based authorization");

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "identity",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    granted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "system"),
                    can_delegate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions",
                        column: x => x.permission_id,
                        principalSchema: "identity",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Many-to-many relationship between roles and permissions");

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    assigned_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "system"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.CheckConstraint("ck_user_roles_expires_at", "expires_at IS NULL OR expires_at > assigned_at");
                    table.ForeignKey(
                        name: "fk_user_roles_roles",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Many-to-many relationship between users and roles");

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[] { new Guid("0b1c2d3e-4f5a-6b7c-8d9e-0f1a2b3c4d5e"), "Fraud", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage fraud detection rules", true, "fraud:rules:manage", "admin" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "name", "scope" },
                values: new object[] { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), "Transaction", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View own transactions", "transactions:read:self", "client" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[,]
                {
                    { new Guid("1c2d3e4f-5a6b-7c8d-9e0f-1a2b3c4d5e6f"), "Compliance", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generate compliance reports", true, "compliance:reports:generate", "compliance" },
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), "Transaction", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View all transactions", true, "transactions:read:all", "admin" },
                    { new Guid("2d3e4f5a-6b7c-8d9e-0f1a-2b3c4d5e6f7a"), "Compliance", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View audit logs", true, "compliance:audit:read", "compliance" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "name", "scope" },
                values: new object[] { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), "Transaction", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create transactions", "transactions:create", "client" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[] { new Guid("3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c8d"), "Transaction", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reverse transactions", true, "transactions:reverse", "admin" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "name", "scope" },
                values: new object[,]
                {
                    { new Guid("3e4f5a6b-7c8d-9e0f-1a2b-3c4d5e6f7a8b"), "User", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View own profile", "users:read:self", "client" },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), "Investment", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View own investments", "investments:read:self", "client" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[] { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), "User", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View all user profiles", true, "users:read:all", "admin" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "name", "scope" },
                values: new object[] { new Guid("5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d"), "User", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Update own profile", "users:update:self", "client" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[] { new Guid("5c6d7e8f-9a0b-1c2d-3e4f-5a6b7c8d9e0f"), "Investment", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View all investments", true, "investments:read:all", "advisor" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "name", "scope" },
                values: new object[] { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), "Account", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View own accounts", "accounts:read:self", "client" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[,]
                {
                    { new Guid("6b7c8d9e-0f1a-2b3c-4d5e-6f7a8b9c0d1e"), "User", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Update any user profile", true, "users:update:all", "admin" },
                    { new Guid("6d7e8f9a-0b1c-2d3e-4f5a-6b7c8d9e0f1a"), "Investment", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Recommend investments", true, "investments:recommend", "advisor" },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), "Account", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View all accounts", true, "accounts:read:all", "admin" },
                    { new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), "Account", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Create new accounts", true, "accounts:create", "admin" },
                    { new Guid("7c8d9e0f-1a2b-3c4d-5e6f-7a8b9c0d1e2f"), "User", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage user roles", true, "users:roles:manage", "admin" },
                    { new Guid("7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b"), "Investment", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage investment portfolios", true, "investments:manage:portfolio", "advisor" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "name", "scope" },
                values: new object[] { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), "Account", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Update own account", "accounts:update:self", "client" });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "permissions",
                columns: new[] { "id", "category", "created_at", "description", "is_sensitive", "name", "scope" },
                values: new object[,]
                {
                    { new Guid("8f9a0b1c-2d3e-4f5a-6b7c-8d9e0f1a2b3c"), "Fraud", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "View fraud alerts", true, "fraud:alerts:read", "security" },
                    { new Guid("9a0b1c2d-3e4f-5a6b-7c8d-9e0f1a2b3c4d"), "Fraud", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Review suspicious transactions", true, "fraud:transactions:review", "security" },
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), "Account", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Update any account", true, "accounts:update:all", "admin" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "roles",
                columns: new[] { "id", "can_be_assigned", "category", "created_at", "description", "is_system_role", "name", "priority" },
                values: new object[,]
                {
                    { new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), true, "Security", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fraud detection and prevention specialist", true, "FraudAnalyst", 75 },
                    { new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), true, "Compliance", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Regulatory compliance officer", true, "ComplianceOfficer", 90 },
                    { new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), true, "Admin", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Financial operations administrator", true, "FinanceAdmin", 80 },
                    { new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, "Admin", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator with full access", true, "SuperAdmin", 100 },
                    { new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), true, "Client", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Investment account holder", true, "Investor", 20 },
                    { new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), true, "Client", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High-value premium investor", true, "PremiumInvestor", 30 },
                    { new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), true, "Client", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Business/corporate investor", true, "BusinessInvestor", 40 },
                    { new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), true, "Advisor", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Registered financial advisor", true, "FinancialAdvisor", 60 },
                    { new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), true, "Management", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Portfolio and wealth manager", true, "WealthManager", 70 },
                    { new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), true, "Client", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Basic financial planning client", true, "Client", 10 },
                    { new Guid("f2a1b0c9-d8e7-6f5a-4b3c-2d1e0f9a8b7c"), true, "Support", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Customer support agent", true, "SupportAgent", 50 }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id", "granted_at", "granted_by" },
                values: new object[,]
                {
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8f9a0b1c-2d3e-4f5a-6b7c-8d9e0f1a2b3c"), new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("9a0b1c2d-3e4f-5a6b-7c8d-9e0f1a2b3c4d"), new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("1c2d3e4f-5a6b-7c8d-9e0f-1a2b3c4d5e6f"), new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2d3e4f5a-6b7c-8d9e-0f1a-2b3c4d5e6f7a"), new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c8d"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6b7c8d9e-0f1a-2b3c-4d5e-6f7a8b9c0d1e"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7c8d9e0f-1a2b-3c4d-5e6f-7a8b9c0d1e2f"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id", "can_delegate", "granted_at", "granted_by" },
                values: new object[,]
                {
                    { new Guid("0b1c2d3e-4f5a-6b7c-8d9e-0f1a2b3c4d5e"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("1c2d3e4f-5a6b-7c8d-9e0f-1a2b3c4d5e6f"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2d3e4f5a-6b7c-8d9e-0f1a-2b3c4d5e6f7a"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c8d"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3e4f5a6b-7c8d-9e0f-1a2b-3c4d5e6f7a8b"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5c6d7e8f-9a0b-1c2d-3e4f-5a6b7c8d9e0f"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6b7c8d9e-0f1a-2b3c-4d5e-6f7a8b9c0d1e"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6d7e8f9a-0b1c-2d3e-4f5a-6b7c8d9e0f1a"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7c8d9e0f-1a2b-3c4d-5e6f-7a8b9c0d1e2f"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8f9a0b1c-2d3e-4f5a-6b7c-8d9e0f1a2b3c"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("9a0b1c2d-3e4f-5a6b-7c8d-9e0f1a2b3c4d"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" }
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id", "granted_at", "granted_by" },
                values: new object[,]
                {
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3e4f5a6b-7c8d-9e0f-1a2b-3c4d5e6f7a8b"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), new Guid("a3b4c5d6-e7f8-9a0b-c1d2-e3f4a5b6c7d8"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3e4f5a6b-7c8d-9e0f-1a2b-3c4d5e6f7a8b"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), new Guid("b9c8d7e6-f5a4-b3c2-d1e0-f9a8b7c6d5e4"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3e4f5a6b-7c8d-9e0f-1a2b-3c4d5e6f7a8b"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5c6d7e8f-9a0b-1c2d-3e4f-5a6b7c8d9e0f"), new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6d7e8f9a-0b1c-2d3e-4f5a-6b7c8d9e0f1a"), new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b"), new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5c6d7e8f-9a0b-1c2d-3e4f-5a6b7c8d9e0f"), new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6d7e8f9a-0b1c-2d3e-4f5a-6b7c8d9e0f1a"), new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b"), new Guid("e7f8a9b0-c1d2-3e4f-5a6b-7c8d9e0f1a2b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("3e4f5a6b-7c8d-9e0f-1a2b-3c4d5e6f7a8b"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), new Guid("e8a7d5f2-1b4c-4a9d-8f3e-6c5b9a8d7f1c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("4f5a6b7c-8d9e-0f1a-2b3c-4d5e6f7a8b9c"), new Guid("f2a1b0c9-d8e7-6f5a-4b3c-2d1e0f9a8b7c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" },
                    { new Guid("6b7c8d9e-0f1a-2b3c-4d5e-6f7a8b9c0d1e"), new Guid("f2a1b0c9-d8e7-6f5a-4b3c-2d1e0f9a8b7c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system:seed" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_assigned_advisor",
                schema: "identity",
                table: "users",
                column: "assigned_financial_advisor",
                filter: "assigned_financial_advisor IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_created_kyc",
                schema: "identity",
                table: "users",
                columns: new[] { "created_at", "kyc_status" });

            migrationBuilder.CreateIndex(
                name: "ix_users_flagged_for_review",
                schema: "identity",
                table: "users",
                column: "is_flagged_for_review");

            migrationBuilder.CreateIndex(
                name: "ix_users_lastlogin_active",
                schema: "identity",
                table: "users",
                columns: new[] { "last_login_at", "is_active" },
                filter: "last_login_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_risk_composite",
                schema: "identity",
                table: "users",
                columns: new[] { "risk_level", "is_flagged_for_review", "kyc_status" });

            migrationBuilder.CreateIndex(
                name: "ix_users_risk_level",
                schema: "identity",
                table: "users",
                column: "risk_level");

            migrationBuilder.CreateIndex(
                name: "ix_users_subscription_tier",
                schema: "identity",
                table: "users",
                column: "subscription_tier");

            migrationBuilder.CreateIndex(
                name: "ix_users_tier_risk",
                schema: "identity",
                table: "users",
                columns: new[] { "subscription_tier", "risk_level" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_refresh_tokens_expiry",
                schema: "identity",
                table: "refresh_tokens",
                sql: "expires_at > created_at");

            migrationBuilder.CreateIndex(
                name: "ix_password_history_user_changed",
                schema: "identity",
                table: "password_history",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_failed_login_attempts_created_at",
                schema: "identity",
                table: "failed_login_attempts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_failed_login_attempts_email",
                schema: "identity",
                table: "failed_login_attempts",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_failed_login_attempts_ip",
                schema: "identity",
                table: "failed_login_attempts",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "ix_failed_login_attempts_ip_time",
                schema: "identity",
                table: "failed_login_attempts",
                columns: new[] { "ip_address", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_failed_login_attempts_user_id",
                schema: "identity",
                table: "failed_login_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_created_at",
                schema: "identity",
                table: "login_logs",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_is_success",
                schema: "identity",
                table: "login_logs",
                column: "is_success");

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_user_id",
                schema: "identity",
                table: "login_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_user_time",
                schema: "identity",
                table: "login_logs",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_permissions_category",
                schema: "identity",
                table: "permissions",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_category_scope",
                schema: "identity",
                table: "permissions",
                columns: new[] { "category", "scope" });

            migrationBuilder.CreateIndex(
                name: "ix_permissions_name",
                schema: "identity",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_permissions_scope",
                schema: "identity",
                table: "permissions",
                column: "scope");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_delegate",
                schema: "identity",
                table: "role_permissions",
                columns: new[] { "role_id", "can_delegate" });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                schema: "identity",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id",
                schema: "identity",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_category",
                schema: "identity",
                table: "roles",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_roles_is_system_role",
                schema: "identity",
                table: "roles",
                column: "is_system_role");

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                schema: "identity",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_priority",
                schema: "identity",
                table: "roles",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_expires_at",
                schema: "identity",
                table: "user_roles",
                column: "expires_at",
                filter: "expires_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_is_active",
                schema: "identity",
                table: "user_roles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "identity",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_active",
                schema: "identity",
                table: "user_roles",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                schema: "identity",
                table: "user_roles",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "failed_login_attempts",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "login_logs",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "ix_users_assigned_advisor",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_created_kyc",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_flagged_for_review",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_lastlogin_active",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_risk_composite",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_risk_level",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_subscription_tier",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_tier_risk",
                schema: "identity",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "ck_refresh_tokens_expiry",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_password_history",
                schema: "identity",
                table: "password_history");

            migrationBuilder.DropIndex(
                name: "ix_password_history_user_changed",
                schema: "identity",
                table: "password_history");

            migrationBuilder.DropColumn(
                name: "address",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "annual_income",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "assigned_compliance_officer",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "assigned_financial_advisor",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "consent_preferences",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "country",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "daily_transaction_limit",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "employment_status",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "flag_reason",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "flagged_at",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "investment_risk_tolerance",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_flagged_for_review",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "kyc_rejection_reason",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "kyc_verified_by",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "monthly_transaction_limit",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "next_review_date",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "postal_code",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "preferred_investment_types",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "primary_investment_goal",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "requires_periodic_review",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "risk_assessed_at",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "risk_assessed_by",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "risk_level",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "risk_notes",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "source_of_funds",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "tax_id_number",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "is_revoked",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "updated_by",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "changed_by_ip",
                schema: "identity",
                table: "password_history");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "identity",
                table: "password_history");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "identity",
                table: "password_history");

            migrationBuilder.DropColumn(
                name: "updated_by",
                schema: "identity",
                table: "password_history");

            migrationBuilder.RenameTable(
                name: "password_history",
                schema: "identity",
                newName: "password_histories",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "ix_password_history_user_id",
                schema: "identity",
                table: "password_histories",
                newName: "ix_password_histories_user_id");

            migrationBuilder.AlterTable(
                name: "users",
                schema: "identity",
                comment: "User accounts for authentication and identity management",
                oldComment: "Users table with enhanced security, KYC/AML, and role-based access control");

            migrationBuilder.AlterTable(
                name: "refresh_tokens",
                schema: "identity",
                comment: "Refresh tokens for JWT authentication and session management");

            migrationBuilder.AlterTable(
                name: "password_histories",
                schema: "identity",
                comment: "Historical password hashes for password reuse prevention");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                comment: "Automatically updated on save",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "two_factor_secret",
                schema: "identity",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "identity",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "language",
                schema: "identity",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldDefaultValue: "en");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "identity",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                schema: "identity",
                table: "users",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "identity",
                table: "users",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                defaultValue: "R",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ZAR");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "identity",
                table: "password_histories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_password_histories",
                schema: "identity",
                table: "password_histories",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_expiry_status",
                schema: "identity",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_password_histories_created_at",
                schema: "identity",
                table: "password_histories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_password_histories_user_password",
                schema: "identity",
                table: "password_histories",
                columns: new[] { "user_id", "password_hash" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_password_histories_created_at",
                schema: "identity",
                table: "password_histories",
                sql: "created_at <= CURRENT_TIMESTAMP");
        }
    }
}
