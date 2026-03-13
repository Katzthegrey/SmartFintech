using SmartFintechFinancial.Modules.Identity.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs;

public record RegisterRequest(
    // Core required fields
    string Email,
    string Password,
    string ConfirmPassword,

    // Personal Information (from User entity)
    string? Phone = null,
    string? FirstName = null,
    string? LastName = null,
    DateTime? DateOfBirth = null,
    string? Address = null,
    string? City = null,
    string? PostalCode = null,
    string? Country = null,

    // Financial Information (from User entity)
    decimal? AnnualIncome = null,
    string? EmploymentStatus = null, // Employed, SelfEmployed, Retired, Student, Unemployed
    string? SourceOfFunds = null, // Salary, Investments, Business, Inheritance, Savings
    string? TaxIdNumber = null, // SSN, SIN, etc. - will be encrypted

    // Registration type determines initial role assignment
    string RegistrationType = "client", // client, investor, business, advisor

    // Investment Preferences 
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    RiskTolerance? InvestmentRiskTolerance = null,

    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    InvestmentGoal? PrimaryInvestmentGoal = null,

    // Investment Preferences (from User entity)
    string? PreferredInvestmentTypes = null, // Stocks, Bonds, ETFs, Real Estate, Crypto

    // Consent & Compliance (from User entity)
    bool? ConsentGiven = null,
    bool? MarketingOptIn = null,
    string? ConsentPreferences = null, // JSON string for granular consent

    // Currency & Localization (from User entity)
    string Currency = "ZAR", // Default South African Rand
    string Language = "en", // Default English

    // KYC/AML Additional Fields
    string? Nationality = null,
    string? Occupation = null,
    string? EmployerName = null,
    bool? IsPoliticallyExposed = null,
    string? ExpectedMonthlyTransactions = null, // Low, Medium, High

    // For Business/Corporate registration
    string? BusinessName = null,
    string? BusinessRegistrationNumber = null,
    string? BusinessType = null, // Pty Ltd, Inc, LLC, etc.
    string? BusinessIndustry = null,
    int? BusinessYearEstablished = null,
    string? BusinessWebsite = null,

    // For Advisor registration (requires additional verification)
    string? AdvisorLicenseNumber = null,
    string? AdvisorRegulatoryBody = null, // FSCA, SEC, etc.
    DateTime? AdvisorLicenseExpiry = null,

    // Referral Information
    string? ReferralCode = null,
    string? CampaignSource = null, // Google, Facebook, Referral, etc.

    // Device/Client Information (for fraud prevention)
    string? DeviceFingerprint = null,
    string? UserAgent = null
)
{
    // Helper method to validate registration type
    public bool IsValidRegistrationType()
    {
        var validTypes = new[] { "client", "investor", "business", "advisor" };
        return validTypes.Contains(RegistrationType?.ToLowerInvariant());
    }

    // Helper method to determine if this is a business registration
    public bool IsBusinessRegistration =>
        RegistrationType?.Equals("business", StringComparison.OrdinalIgnoreCase) == true;

    // Helper method to determine if this is an advisor registration
    public bool IsAdvisorRegistration =>
        RegistrationType?.Equals("advisor", StringComparison.OrdinalIgnoreCase) == true;

    // Helper method to get full name
    public string? FullName =>
        string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? null
            : $"{FirstName} {LastName}".Trim();

    // Helper method to check if KYC info is complete for investor registration
    public bool HasMinimumKycInfo()
    {
        if (RegistrationType?.ToLowerInvariant() != "investor")
            return true; // Only investors need KYC at registration

        return !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               DateOfBirth.HasValue &&
               !string.IsNullOrWhiteSpace(Country) &&
               !string.IsNullOrWhiteSpace(TaxIdNumber);
    }
}