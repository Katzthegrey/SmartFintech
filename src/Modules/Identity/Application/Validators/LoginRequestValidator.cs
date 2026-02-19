using FluentValidation;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;

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

        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "OR", "AND", "'", ";", "--", "#" };
        return !sqlKeywords.Any(keyword =>
            input.ToUpperInvariant().Contains(keyword));
    }
}