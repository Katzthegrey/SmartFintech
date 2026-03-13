using FluentValidation;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using System.Text.RegularExpressions;

namespace SmartFintechFinancial.Modules.Identity.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required")
            .MaximumLength(255)
            .Must(NotContainSqlKeywords).WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(128);

        RuleFor(x => x.TwoFactorCode)
            .Matches(@"^\d{6}$")
            .When(x => !string.IsNullOrEmpty(x.TwoFactorCode))
            .WithMessage("Two-factor code must be 6 digits");
    }

    private bool NotContainSqlKeywords(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        // SQL keywords to check for as whole words
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "OR", "AND" };

        // SQL symbols to check for anywhere
        var sqlSymbols = new[] { "'", ";", "--", "#" };

        // Check for SQL symbols first (these are always bad anywhere)
        foreach (var symbol in sqlSymbols)
        {
            if (input.Contains(symbol))
            {
                return false;
            }
        }

        // Check for SQL keywords as whole words using regex word boundaries
        var upperInput = input.ToUpperInvariant();
        foreach (var keyword in sqlKeywords)
        {
            // \b means word boundary - ensures we match whole words only
            var pattern = $@"\b{keyword}\b";
            if (Regex.IsMatch(upperInput, pattern))
            {
                return false;
            }
        }

        return true;
    }
}