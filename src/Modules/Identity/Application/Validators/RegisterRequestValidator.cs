using FluentValidation;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;

namespace SmartFintechFinancial.Modules.Identity.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required")
            .MaximumLength(255)
            .Must(BeAValidEmailDomain).WithMessage("Invalid email domain")
            .Must(NotContainSqlKeywords).WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters") // Increased from 8
            .MaximumLength(128)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one number")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")
            .WithMessage("Password must contain at least one special character")
            .Must(NotBeCommonPassword).WithMessage("Password is too common")
            .Must(NotContainPersonalInfo).WithMessage("Password cannot contain personal information");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number must be in E.164 format")
            .MaximumLength(20);
    }

    private bool BeAValidEmailDomain(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        // Reject disposable/temporary email domains
        var disposableDomains = new[] { "tempmail", "10minutemail", "guerrillamail", "mailinator" };
        var domain = email.Split('@').LastOrDefault()?.ToLower();

        if (string.IsNullOrEmpty(domain))
            return false;

        return !disposableDomains.Any(d => domain.Contains(d));
    }

    private bool NotContainSqlKeywords(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "OR", "AND", "'", ";", "--" };
        return !sqlKeywords.Any(keyword =>
            input.ToUpperInvariant().Contains(keyword));
    }

    private bool NotBeCommonPassword(string password)
    {
        var commonPasswords = new[]
        {
            "password", "123456", "qwerty", "letmein", "welcome",
            "monkey", "dragon", "baseball", "football", "mustang"
        };

        return !commonPasswords.Any(p =>
            password.Equals(p, StringComparison.OrdinalIgnoreCase));
    }

    private bool NotContainPersonalInfo(string password)
    {
        // In real implementation, compare against user data
        // This is a simplified version
        var personalPatterns = new[] { "password", "123", "admin", "user" };
        return !personalPatterns.Any(p =>
            password.ToLowerInvariant().Contains(p));
    }
}