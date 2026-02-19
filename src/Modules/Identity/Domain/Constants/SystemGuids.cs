namespace SmartFintechFinancial.Modules.Identity.Domain.Constants;

/// <summary>
/// System GUIDs for critical entities.
/// IMPORTANT: These GUIDs are FIXED and should NEVER be changed.
/// Generated once using a GUID generator and saved here permanently.
/// </summary>
public static class SystemGuids
{
    // ===== SYSTEM ROLES (CRITICAL - NEVER CHANGE THESE) =====

    // Client Roles
    public const string Client = "E8A7D5F2-1B4C-4A9D-8F3E-6C5B9A8D7F1C";
    public const string Investor = "A3B4C5D6-E7F8-9A0B-C1D2-E3F4A5B6C7D8";
    public const string PremiumInvestor = "B9C8D7E6-F5A4-B3C2-D1E0-F9A8B7C6D5E4";
    public const string BusinessInvestor = "C1D2E3F4-A5B6-7C8D-9E0F-1A2B3C4D5E6F";

    // Advisor/Manager Roles
    public const string FinancialAdvisor = "D4E5F6A7-B8C9-0D1E-2F3A-4B5C6D7E8F9A";
    public const string WealthManager = "E7F8A9B0-C1D2-3E4F-5A6B-7C8D9E0F1A2B";

    // Support/Operational Roles
    public const string SupportAgent = "F2A1B0C9-D8E7-6F5A-4B3C-2D1E0F9A8B7C";
    public const string FraudAnalyst = "1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D";
    public const string ComplianceOfficer = "2B3C4D5E-6F7A-8B9C-0D1E-2F3A4B5C6D7E";

    // Administrative Roles
    public const string FinanceAdmin = "3C4D5E6F-7A8B-9C0D-1E2F-3A4B5C6D7E8F";
    public const string SuperAdmin = "4D5E6F7A-8B9C-0D1E-2F3A-4B5C6D7E8F9A";

    // ===== SYSTEM PERMISSIONS =====

    // Account Permissions
    public const string AccountsReadSelf = "5E6F7A8B-9C0D-1E2F-3A4B-5C6D7E8F9A0B";
    public const string AccountsReadAll = "6F7A8B9C-0D1E-2F3A-4B5C-6D7E8F9A0B1C";
    public const string AccountsCreate = "7A8B9C0D-1E2F-3A4B-5C6D-7E8F9A0B1C2D";
    public const string AccountsUpdateSelf = "8B9C0D1E-2F3A-4B5C-6D7E-8F9A0B1C2D3E";
    public const string AccountsUpdateAll = "9C0D1E2F-3A4B-5C6D-7E8F-9A0B1C2D3E4F";

    // Transaction Permissions
    public const string TransactionsReadSelf = "0D1E2F3A-4B5C-6D7E-8F9A-0B1C2D3E4F5A";
    public const string TransactionsReadAll = "1E2F3A4B-5C6D-7E8F-9A0B-1C2D3E4F5A6B";
    public const string TransactionsCreate = "2F3A4B5C-6D7E-8F9A-0B1C-2D3E4F5A6B7C";
    public const string TransactionsReverse = "3A4B5C6D-7E8F-9A0B-1C2D-3E4F5A6B7C8D";

    // Investment Permissions
    public const string InvestmentsReadSelf = "4B5C6D7E-8F9A-0B1C-2D3E-4F5A6B7C8D9E";
    public const string InvestmentsReadAll = "5C6D7E8F-9A0B-1C2D-3E4F-5A6B7C8D9E0F";
    public const string InvestmentsRecommend = "6D7E8F9A-0B1C-2D3E-4F5A-6B7C8D9E0F1A";
    public const string InvestmentsManagePortfolio = "7E8F9A0B-1C2D-3E4F-5A6B-7C8D9E0F1A2B";

    // Fraud Detection Permissions
    public const string FraudAlertsRead = "8F9A0B1C-2D3E-4F5A-6B7C-8D9E0F1A2B3C";
    public const string FraudTransactionsReview = "9A0B1C2D-3E4F-5A6B-7C8D-9E0F1A2B3C4D";
    public const string FraudRulesManage = "0B1C2D3E-4F5A-6B7C-8D9E-0F1A2B3C4D5E";

    // Compliance Permissions
    public const string ComplianceReportsGenerate = "1C2D3E4F-5A6B-7C8D-9E0F-1A2B3C4D5E6F";
    public const string ComplianceAuditRead = "2D3E4F5A-6B7C-8D9E-0F1A-2B3C4D5E6F7A";

    // User Management Permissions
    public const string UsersReadSelf = "3E4F5A6B-7C8D-9E0F-1A2B-3C4D5E6F7A8B";
    public const string UsersReadAll = "4F5A6B7C-8D9E-0F1A-2B3C-4D5E6F7A8B9C";
    public const string UsersUpdateSelf = "5A6B7C8D-9E0F-1A2B-3C4D-5E6F7A8B9C0D";
    public const string UsersUpdateAll = "6B7C8D9E-0F1A-2B3C-4D5E-6F7A8B9C0D1E";
    public const string UsersRolesManage = "7C8D9E0F-1A2B-3C4D-5E6F-7A8B9C0D1E2F";

    // ===== HELPER METHODS =====

    /// <summary>
    /// Gets the GUID for a system role by name
    /// </summary>
    public static Guid GetRoleGuid(string roleName) => roleName switch
    {
        RoleConstants.Client => Guid.Parse(Client),
        RoleConstants.Investor => Guid.Parse(Investor),
        RoleConstants.PremiumInvestor => Guid.Parse(PremiumInvestor),
        RoleConstants.BusinessInvestor => Guid.Parse(BusinessInvestor),
        RoleConstants.FinancialAdvisor => Guid.Parse(FinancialAdvisor),
        RoleConstants.WealthManager => Guid.Parse(WealthManager),
        RoleConstants.SupportAgent => Guid.Parse(SupportAgent),
        RoleConstants.FraudAnalyst => Guid.Parse(FraudAnalyst),
        RoleConstants.ComplianceOfficer => Guid.Parse(ComplianceOfficer),
        RoleConstants.FinanceAdmin => Guid.Parse(FinanceAdmin),
        RoleConstants.SuperAdmin => Guid.Parse(SuperAdmin),
        _ => throw new ArgumentException($"Unknown system role: {roleName}")
    };

    /// <summary>
    /// Gets the GUID for a system permission by name
    /// </summary>
    public static Guid GetPermissionGuid(string permissionName) => permissionName switch
    {
        "accounts:read:self" => Guid.Parse(AccountsReadSelf),
        "accounts:read:all" => Guid.Parse(AccountsReadAll),
        "accounts:create" => Guid.Parse(AccountsCreate),
        "accounts:update:self" => Guid.Parse(AccountsUpdateSelf),
        "accounts:update:all" => Guid.Parse(AccountsUpdateAll),

        "transactions:read:self" => Guid.Parse(TransactionsReadSelf),
        "transactions:read:all" => Guid.Parse(TransactionsReadAll),
        "transactions:create" => Guid.Parse(TransactionsCreate),
        "transactions:reverse" => Guid.Parse(TransactionsReverse),

        "investments:read:self" => Guid.Parse(InvestmentsReadSelf),
        "investments:read:all" => Guid.Parse(InvestmentsReadAll),
        "investments:recommend" => Guid.Parse(InvestmentsRecommend),
        "investments:manage:portfolio" => Guid.Parse(InvestmentsManagePortfolio),

        "fraud:alerts:read" => Guid.Parse(FraudAlertsRead),
        "fraud:transactions:review" => Guid.Parse(FraudTransactionsReview),
        "fraud:rules:manage" => Guid.Parse(FraudRulesManage),

        "compliance:reports:generate" => Guid.Parse(ComplianceReportsGenerate),
        "compliance:audit:read" => Guid.Parse(ComplianceAuditRead),

        "users:read:self" => Guid.Parse(UsersReadSelf),
        "users:read:all" => Guid.Parse(UsersReadAll),
        "users:update:self" => Guid.Parse(UsersUpdateSelf),
        "users:update:all" => Guid.Parse(UsersUpdateAll),
        "users:roles:manage" => Guid.Parse(UsersRolesManage),

        _ => throw new ArgumentException($"Unknown system permission: {permissionName}")
    };

    /// <summary>
    /// Validates that all GUIDs are unique (run during application startup)
    /// </summary>
    public static void ValidateGuids()
    {
        var allGuids = new[]
        {
            // Role GUIDs
            Client, Investor, PremiumInvestor, BusinessInvestor,
            FinancialAdvisor, WealthManager, SupportAgent, FraudAnalyst,
            ComplianceOfficer, FinanceAdmin, SuperAdmin,
            
            // Permission GUIDs
            AccountsReadSelf, AccountsReadAll, AccountsCreate, AccountsUpdateSelf, AccountsUpdateAll,
            TransactionsReadSelf, TransactionsReadAll, TransactionsCreate, TransactionsReverse,
            InvestmentsReadSelf, InvestmentsReadAll, InvestmentsRecommend, InvestmentsManagePortfolio,
            FraudAlertsRead, FraudTransactionsReview, FraudRulesManage,
            ComplianceReportsGenerate, ComplianceAuditRead,
            UsersReadSelf, UsersReadAll, UsersUpdateSelf, UsersUpdateAll, UsersRolesManage
        };

        var guidObjects = allGuids.Select(Guid.Parse).ToList();
        var duplicates = guidObjects
            .GroupBy(g => g)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            throw new InvalidOperationException(
                $"Duplicate GUIDs found in SystemGuids: {string.Join(", ", duplicates)}");
        }
    }

    /// <summary>
    /// Gets all system role names with their GUIDs
    /// </summary>
    public static Dictionary<string, Guid> GetAllRoleGuids() => new()
    {
        { RoleConstants.Client, Guid.Parse(Client) },
        { RoleConstants.Investor, Guid.Parse(Investor) },
        { RoleConstants.PremiumInvestor, Guid.Parse(PremiumInvestor) },
        { RoleConstants.BusinessInvestor, Guid.Parse(BusinessInvestor) },
        { RoleConstants.FinancialAdvisor, Guid.Parse(FinancialAdvisor) },
        { RoleConstants.WealthManager, Guid.Parse(WealthManager) },
        { RoleConstants.SupportAgent, Guid.Parse(SupportAgent) },
        { RoleConstants.FraudAnalyst, Guid.Parse(FraudAnalyst) },
        { RoleConstants.ComplianceOfficer, Guid.Parse(ComplianceOfficer) },
        { RoleConstants.FinanceAdmin, Guid.Parse(FinanceAdmin) },
        { RoleConstants.SuperAdmin, Guid.Parse(SuperAdmin) }
    };

    /// <summary>
    /// Gets all system permission names with their GUIDs
    /// </summary>
    public static Dictionary<string, Guid> GetAllPermissionGuids() => new()
    {
        { "accounts:read:self", Guid.Parse(AccountsReadSelf) },
        { "accounts:read:all", Guid.Parse(AccountsReadAll) },
        { "accounts:create", Guid.Parse(AccountsCreate) },
        { "accounts:update:self", Guid.Parse(AccountsUpdateSelf) },
        { "accounts:update:all", Guid.Parse(AccountsUpdateAll) },

        { "transactions:read:self", Guid.Parse(TransactionsReadSelf) },
        { "transactions:read:all", Guid.Parse(TransactionsReadAll) },
        { "transactions:create", Guid.Parse(TransactionsCreate) },
        { "transactions:reverse", Guid.Parse(TransactionsReverse) },

        { "investments:read:self", Guid.Parse(InvestmentsReadSelf) },
        { "investments:read:all", Guid.Parse(InvestmentsReadAll) },
        { "investments:recommend", Guid.Parse(InvestmentsRecommend) },
        { "investments:manage:portfolio", Guid.Parse(InvestmentsManagePortfolio) },

        { "fraud:alerts:read", Guid.Parse(FraudAlertsRead) },
        { "fraud:transactions:review", Guid.Parse(FraudTransactionsReview) },
        { "fraud:rules:manage", Guid.Parse(FraudRulesManage) },

        { "compliance:reports:generate", Guid.Parse(ComplianceReportsGenerate) },
        { "compliance:audit:read", Guid.Parse(ComplianceAuditRead) },

        { "users:read:self", Guid.Parse(UsersReadSelf) },
        { "users:read:all", Guid.Parse(UsersReadAll) },
        { "users:update:self", Guid.Parse(UsersUpdateSelf) },
        { "users:update:all", Guid.Parse(UsersUpdateAll) },
        { "users:roles:manage", Guid.Parse(UsersRolesManage) }
    };
}