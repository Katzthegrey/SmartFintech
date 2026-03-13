using FluentValidation;
using SmartFintechFinancial.Modules.Identity.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Diagnostics; // Add this using

namespace SmartFintechFinancial.Modules.Identity.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly ILogger<RegisterRequestValidator> _logger;
    public RegisterRequestValidator(ILogger<RegisterRequestValidator> logger)
    {
        _logger = logger;

        // Debug output at constructor
        Debug.WriteLine("==========================================");
        Debug.WriteLine($"VALIDATOR CONSTRUCTOR CALLED at {DateTime.Now:HH:mm:ss.fff}");
        Debug.WriteLine("==========================================");

        RuleFor(x => x.Email)
            // LOG FIRST - this will run before any other validations
            .Must((request, email) =>
            {
                Debug.WriteLine("--- EMAIL VALIDATION ---");
                Debug.WriteLine($"Raw email value: '{email}'");
                Debug.WriteLine($"Email length: {email?.Length ?? 0}");
                Debug.WriteLine($"Contains @: {email?.Contains('@') ?? false}");
                Debug.WriteLine($"Domain part: {email?.Split('@').LastOrDefault() ?? "none"}");

                _logger.LogInformation("=== STARTING VALIDATION ===");
                _logger.LogInformation("Email value received: '{Email}'", email);
                _logger.LogInformation("Email length: {Length}", email?.Length ?? 0);
                _logger.LogInformation("Email contains @: {ContainsAt}", email?.Contains('@') ?? false);
                return true; // Always passes - just for logging
            })
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email is required")
            .MaximumLength(255)
            .Must(BeAValidEmailDomain).WithMessage("Invalid email domain")
            .Must(NotContainSqlKeywords).WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            // LOG FIRST
            .Must((request, password) =>
            {
                Debug.WriteLine("--- PASSWORD VALIDATION ---");
                Debug.WriteLine($"Password length: {password?.Length ?? 0}");
                Debug.WriteLine($"Has uppercase: {password?.Any(char.IsUpper) ?? false}");
                Debug.WriteLine($"Has lowercase: {password?.Any(char.IsLower) ?? false}");
                Debug.WriteLine($"Has number: {password?.Any(char.IsDigit) ?? false}");
                Debug.WriteLine($"Has special: {password?.Any(c => "!@#$%^&*()_+-=[]{};':\"\\|,.<>/?".Contains(c)) ?? false}");

                _logger.LogInformation("Password value received: [REDACTED]");
                _logger.LogInformation("Password length: {Length}", password?.Length ?? 0);
                _logger.LogInformation("Password contains uppercase: {HasUpper}", password?.Any(char.IsUpper) ?? false);
                _logger.LogInformation("Password contains lowercase: {HasLower}", password?.Any(char.IsLower) ?? false);
                _logger.LogInformation("Password contains number: {HasNumber}", password?.Any(char.IsDigit) ?? false);
                _logger.LogInformation("Password contains special: {HasSpecial}",
                    password?.Any(c => "!@#$%^&*()_+-=[]{};':\"\\|,.<>/?".Contains(c)) ?? false);
                return true;
            })
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one number")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")
            .WithMessage("Password must contain at least one special character")
            .Must(NotBeCommonPassword).WithMessage("Password is too common")
            .Must((request, password) => NotContainPersonalInfo(request, password))
            .WithMessage("Password cannot contain personal information"); 

        RuleFor(x => x.ConfirmPassword)
            // LOG FIRST
            .Must((request, confirmPassword) =>
            {
                Debug.WriteLine("--- CONFIRM PASSWORD ---");
                Debug.WriteLine($"ConfirmPassword length: {confirmPassword?.Length ?? 0}");
                Debug.WriteLine($"Passwords match: {request.Password == confirmPassword}");

                _logger.LogInformation("ConfirmPassword length: {Length}", confirmPassword?.Length ?? 0);
                _logger.LogInformation("Passwords match: {Matches}", request.Password == confirmPassword);
                return true;
            })
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.RegistrationType)
            // LOG FIRST
            .Must((request, type) =>
            {
                Debug.WriteLine("--- REGISTRATION TYPE ---");
                Debug.WriteLine($"RegistrationType value: '{type}'");
                Debug.WriteLine($"Is in allowed list: {new[] { "client", "investor", "advisor", "business", "admin", "premium" }.Contains(type)}");

                _logger.LogInformation("RegistrationType value: '{Type}'", type);
                _logger.LogInformation("RegistrationType is in allowed list: {IsValid}",
                    new[] { "client", "investor", "advisor", "business", "admin", "premium" }.Contains(type));
                return true;
            })
            .NotEmpty().WithMessage("Registration type is required")
            .Must(type => new[] { "client", "investor", "advisor", "business", "admin", "premium" }.Contains(type))
            .WithMessage("Registration type must be client, admin, investor, premium,  advisor, or business");

        RuleFor(x => x.ConsentGiven)
            // LOG FIRST
            .Must((request, consent) =>
            {
                Debug.WriteLine("--- CONSENT GIVEN ---");
                Debug.WriteLine($"ConsentGiven value: {consent}");

                _logger.LogInformation("ConsentGiven value: {Consent}", consent);
                return true;
            })
            .Equal(true).WithMessage("You must consent to terms");

        // Investor-specific required fields
        RuleFor(x => x.AnnualIncome)
            .GreaterThan(0).WithMessage("Annual income is required for investors")
            .When(x => x.RegistrationType == "investor");

        RuleFor(x => x.EmploymentStatus)
            .NotEmpty().WithMessage("Employment status is required for investors")
            .When(x => x.RegistrationType == "investor");

        RuleFor(x => x.SourceOfFunds)
            .NotEmpty().WithMessage("Source of funds is required for investors")
            .When(x => x.RegistrationType == "investor");

        RuleFor(x => x.InvestmentRiskTolerance)
            .NotNull().WithMessage("Investment risk tolerance is required for investors")
            .When(x => x.RegistrationType == "investor");

        RuleFor(x => x.PrimaryInvestmentGoal)
            .NotNull().WithMessage("Primary investment goal is required for investors")
            .When(x => x.RegistrationType == "investor");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number must be in E.164 format")
            .MaximumLength(20);

        
        RuleFor(x => x)
            .Must(request =>
            {
                Debug.WriteLine("==========================================");
                Debug.WriteLine("VALIDATION COMPLETE - ALL RULES EXECUTED");
                Debug.WriteLine($"Final result: Check response for errors");
                Debug.WriteLine("==========================================");

                _logger.LogInformation("=== COMPLETE VALIDATION SUMMARY ===");
                _logger.LogInformation("All validation rules completed");
                return true;
            });

        // rules for RegisterRequestValidator constructor
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .Must(NotContainXssPayloads).WithMessage("First name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .Must(NotContainXssPayloads).WithMessage("Last name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Address)
            .MaximumLength(200)
            .Must(NotContainXssPayloads).When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address contains invalid characters");

        RuleFor(x => x.City)
            .MaximumLength(50)
            .Must(NotContainXssPayloads).When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("City contains invalid characters");

        RuleFor(x => x.Country)
            .MaximumLength(50)
            .Must(NotContainXssPayloads).When(x => !string.IsNullOrEmpty(x.Country))
            .WithMessage("Country contains invalid characters");

        RuleFor(x => x.EmploymentStatus)
            .MaximumLength(50)
            .Must(NotContainXssPayloads).When(x => !string.IsNullOrEmpty(x.EmploymentStatus))
            .WithMessage("Employment status contains invalid characters");

        RuleFor(x => x.SourceOfFunds)
            .MaximumLength(50)
            .Must(NotContainXssPayloads).When(x => !string.IsNullOrEmpty(x.SourceOfFunds))
            .WithMessage("Source of funds contains invalid characters");
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

        var isValid = !disposableDomains.Any(d => domain.Contains(d));

        Debug.WriteLine($"--- EMAIL DOMAIN CHECK ---");
        Debug.WriteLine($"Domain: {domain}");
        Debug.WriteLine($"Is valid: {isValid}");

        return isValid;
    }

    private bool NotContainSqlKeywords(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "UNION", "OR", "AND" };
        var sqlSymbols = new[] { "'", ";", "--" };

        // Check for SQL symbols
        foreach (var symbol in sqlSymbols)
        {
            if (input.Contains(symbol))
            {
                Debug.WriteLine($"--- SQL INJECTION CHECK FAILED ---");
                Debug.WriteLine($"Input contains SQL symbol: {symbol}");
                return false;
            }
        }

        // Check for SQL keywords as whole words using regex word boundaries
        var upperInput = input.ToUpperInvariant();
        foreach (var keyword in sqlKeywords)
        {
            // \b means word boundary - ensures we match whole words only
            var pattern = $@"\b{keyword}\b";
            if (System.Text.RegularExpressions.Regex.IsMatch(upperInput, pattern))
            {
                Debug.WriteLine($"--- SQL INJECTION CHECK FAILED ---");
                Debug.WriteLine($"Input contains SQL keyword: {keyword}");
                return false;
            }
        }

        return true;
    }

    private bool NotBeCommonPassword(string password)
    {
        var commonPasswords = new[]
        {
            "password", "123456", "qwerty", "letmein", "welcome",
            "monkey", "dragon", "baseball", "football", "mustang"
        };

        var isValid = !commonPasswords.Any(p =>
            password.Equals(p, StringComparison.OrdinalIgnoreCase));

        if (!isValid)
        {
            Debug.WriteLine($"--- COMMON PASSWORD CHECK FAILED ---");
            Debug.WriteLine($"Password is too common");
        }

        return isValid;
    }

    private bool NotContainPersonalInfo(RegisterRequest request, string password)
    {
        if (string.IsNullOrEmpty(password))
            return true;

        var lowerPassword = password.ToLowerInvariant();

        Debug.WriteLine($"--- PERSONAL INFO CHECK DETAILS ---");
        Debug.WriteLine($"Checking password: '{password}'");
        Debug.WriteLine($"Lowercase: '{lowerPassword}'");

        // 1. Check against actual user data (most important)
        if (!string.IsNullOrEmpty(request.FirstName))
        {
            var lowerFirstName = request.FirstName.ToLowerInvariant();
            if (lowerPassword.Contains(lowerFirstName) && lowerFirstName.Length >= 3)
            {
                Debug.WriteLine($"   FAILED - contains first name: {request.FirstName}");
                return false;
            }
        }

        if (!string.IsNullOrEmpty(request.LastName))
        {
            var lowerLastName = request.LastName.ToLowerInvariant();
            if (lowerPassword.Contains(lowerLastName) && lowerLastName.Length >= 3)
            {
                Debug.WriteLine($"   FAILED - contains last name: {request.LastName}");
                return false;
            }
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            var emailLocalPart = request.Email.Split('@')[0].ToLowerInvariant();
            if (lowerPassword.Contains(emailLocalPart) && emailLocalPart.Length >= 3)
            {
                Debug.WriteLine($"   FAILED - contains email local part: {emailLocalPart}");
                return false;
            }
        }

        // 2. Check for common weak patterns (but be smarter about it)
        var commonPatterns = new Dictionary<string, string>
    {
        { "password", "Common password pattern" },
        { "qwerty", "Keyboard pattern" },
        { "abc123", "Sequential pattern" },
        { "letmein", "Common phrase" },
        { "welcome", "Common word" }
    };

        foreach (var pattern in commonPatterns)
        {
            if (lowerPassword.Contains(pattern.Key) && pattern.Key.Length >= 4)
            {
                Debug.WriteLine($"   FAILED - {pattern.Value}: {pattern.Key}");
                return false;
            }
        }

        // 3. Check for sequential numbers (more than 2 in a row)
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"\d{3,}"))
        {
            Debug.WriteLine($"   FAILED - contains 3+ sequential numbers");
            return false;
        }

        // 4. Check for repeated characters (aaa, 111)
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"(.)\1{2,}"))
        {
            Debug.WriteLine($"   FAILED - contains repeated characters");
            return false;
        }

        // 5. Check for keyboard patterns (qwerty, asdf)
        var keyboardPatterns = new[] { "qwerty", "asdfgh", "zxcvbn", "qwertyuiop", "asdfghjkl", "zxcvbnm" };
        foreach (var pattern in keyboardPatterns)
        {
            if (lowerPassword.Contains(pattern))
            {
                Debug.WriteLine($"   FAILED - contains keyboard pattern: {pattern}");
                return false;
            }
        }

        // 6. Check for year patterns (1990, 2024) that might be birth years
        if (!string.IsNullOrEmpty(request.DateOfBirth?.ToString()))
        {
            var birthYear = request.DateOfBirth.Value.Year.ToString();
            if (lowerPassword.Contains(birthYear))
            {
                Debug.WriteLine($"   FAILED - contains birth year: {birthYear}");
                return false;
            }
        }

        // 7. Check for common substitutions (but don't be too strict)
        var commonSubstitutions = new Dictionary<string, string[]>
    {
        { "admin", new[] { "@dmin", "adm1n", "4dmin" } },
        { "password", new[] { "p@ssword", "p4ssword" } }
    };

        foreach (var kvp in commonSubstitutions)
        {
            foreach (var variant in kvp.Value)
            {
                if (lowerPassword.Contains(variant.ToLowerInvariant()))
                {
                    Debug.WriteLine($"   FAILED - contains common substitution: {variant}");
                    return false;
                }
            }
        }

        Debug.WriteLine($"   PASSED - no personal info found");
        return true;
    }

    // Add this method to your RegisterRequestValidator class
    private bool NotContainXssPayloads(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        var xssPatterns = new[]
        {
        @"<script.*?>.*?</script>",
        @"javascript:",
        @"on\w+\s*=",
        @"<iframe.*?>.*?</iframe>",
        @"<object.*?>.*?</object>",
        @"<embed.*?>.*?</embed>",
        @"<svg.*?>.*?</svg>",
        @"<img.*?on\w+\s*=.*?>",
        @"<body.*?>.*?</body>",
        @"<.*?alert\(.*?\).*?>",
        @"<.*?confirm\(.*?\).*?>",
        @"<.*?prompt\(.*?\).*?>",
        @"<.*?eval\(.*?\).*?>"
    };

        foreach (var pattern in xssPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(input, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                Debug.WriteLine($"--- XSS CHECK FAILED ---");
                Debug.WriteLine($"Input contains XSS pattern: {pattern}");
                return false;
            }
        }

        return true;
    }
}