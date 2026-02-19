using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Domain.Constants
{
    public static class RoleConstants
    {
        // Client Roles
        public const string Client = "Client";                    // Basic client
        public const string Investor = "Investor";                // Investment account holder
        public const string PremiumInvestor = "PremiumInvestor";  // High-value investor
        public const string BusinessInvestor = "BusinessInvestor"; // Business/Corporate investor

        // Advisor/Manager Roles
        public const string FinancialAdvisor = "FinancialAdvisor"; // Registered financial advisor
        public const string WealthManager = "WealthManager";      // Portfolio/wealth manager

        // Support/Operational Roles
        public const string SupportAgent = "SupportAgent";        // Customer support
        public const string FraudAnalyst = "FraudAnalyst";        // Fraud detection specialist
        public const string ComplianceOfficer = "ComplianceOfficer"; // Regulatory compliance

        // Administrative Roles
        public const string FinanceAdmin = "FinanceAdmin";        // Financial operations admin
        public const string SuperAdmin = "SuperAdmin";            // Full system access

        // System Roles (for automation)
        public const string System = "System";                    // Automated/system actions
        public const string API_Client = "API_Client";           // API access clients
    }

    public static class PermissionConstants
    {
        // Account Permissions
        public const string Accounts_Read_Self = "accounts:read:self";
        public const string Accounts_Read_All = "accounts:read:all";
        public const string Accounts_Create = "accounts:create";
        public const string Accounts_Update_Self = "accounts:update:self";
        public const string Accounts_Update_All = "accounts:update:all";
        public const string Accounts_Delete = "accounts:delete";
        public const string Accounts_Suspend = "accounts:suspend";

        // Transaction Permissions
        public const string Transactions_Read_Self = "transactions:read:self";
        public const string Transactions_Read_All = "transactions:read:all";
        public const string Transactions_Create = "transactions:create";
        public const string Transactions_Reverse = "transactions:reverse";
        public const string Transactions_Export = "transactions:export";

        // Investment Permissions
        public const string Investments_Read_Self = "investments:read:self";
        public const string Investments_Read_All = "investments:read:all";
        public const string Investments_Trade = "investments:trade";
        public const string Investments_Recommend = "investments:recommend";
        public const string Investments_Manage_Portfolio = "investments:manage:portfolio";

        // Transfer Permissions
        public const string Transfers_Create_Self = "transfers:create:self";
        public const string Transfers_Create_All = "transfers:create:all";
        public const string Transfers_Approve = "transfers:approve";
        public const string Transfers_Reject = "transfers:reject";
        public const string Transfers_Read_All = "transfers:read:all";

        // User Management Permissions
        public const string Users_Read_Self = "users:read:self";
        public const string Users_Read_All = "users:read:all";
        public const string Users_Update_Self = "users:update:self";
        public const string Users_Update_All = "users:update:all";
        public const string Users_Delete = "users:delete";
        public const string Users_Suspend = "users:suspend";
        public const string Users_Roles_Manage = "users:roles:manage";

        // Fraud Detection Permissions
        public const string Fraud_Alerts_Read = "fraud:alerts:read";
        public const string Fraud_Alerts_Acknowledge = "fraud:alerts:acknowledge";
        public const string Fraud_Alerts_Escalate = "fraud:alerts:escalate";
        public const string Fraud_Transactions_Review = "fraud:transactions:review";
        public const string Fraud_Rules_Manage = "fraud:rules:manage";

        // Compliance Permissions
        public const string Compliance_Reports_Generate = "compliance:reports:generate";
        public const string Compliance_Audit_Read = "compliance:audit:read";
        public const string Compliance_Limits_Manage = "compliance:limits:manage";
        public const string Compliance_Users_Review = "compliance:users:review";

        // System Permissions
        public const string System_Settings = "system:settings";
        public const string System_Health = "system:health";
        public const string System_Maintenance = "system:maintenance";
        public const string System_Audit_Read = "system:audit:read";
    }

    public static class RoleHierarchy
    {
        // Defines which roles can manage which other roles
        public static readonly Dictionary<string, string[]> ManagementRights = new()
    {
        { RoleConstants.SuperAdmin,
            new[] {
                RoleConstants.FinanceAdmin,
                RoleConstants.ComplianceOfficer,
                RoleConstants.WealthManager,
                RoleConstants.FinancialAdvisor,
                RoleConstants.FraudAnalyst,
                RoleConstants.SupportAgent
            }
        },

        { RoleConstants.FinanceAdmin,
            new[] {
                RoleConstants.WealthManager,
                RoleConstants.FinancialAdvisor,
                RoleConstants.SupportAgent
            }
        },

        { RoleConstants.ComplianceOfficer,
            new[] {
                RoleConstants.FraudAnalyst,
                RoleConstants.SupportAgent
            }
        },

        { RoleConstants.WealthManager,
            new[] {
                RoleConstants.FinancialAdvisor
            }
        },

        { RoleConstants.FinancialAdvisor,
            new[] {
                RoleConstants.Client,
                RoleConstants.Investor,
                RoleConstants.PremiumInvestor,
                RoleConstants.BusinessInvestor
            }
        }
    };

        // Defines role categories
        public static readonly Dictionary<string, string> RoleCategories = new()
    {
        { RoleConstants.Client, "Client" },
        { RoleConstants.Investor, "Client" },
        { RoleConstants.PremiumInvestor, "Client" },
        { RoleConstants.BusinessInvestor, "Client" },
        { RoleConstants.FinancialAdvisor, "Advisor" },
        { RoleConstants.WealthManager, "Management" },
        { RoleConstants.SupportAgent, "Support" },
        { RoleConstants.FraudAnalyst, "Security" },
        { RoleConstants.ComplianceOfficer, "Compliance" },
        { RoleConstants.FinanceAdmin, "Admin" },
        { RoleConstants.SuperAdmin, "Admin" }
    };
    }
}
