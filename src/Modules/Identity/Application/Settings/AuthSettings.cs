using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.Settings;

public class AuthSettings
{
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int AccountLockoutMinutes { get; set; } = 15;
    public bool RequireEmailVerification { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public bool EnableBruteForceProtection { get; set; } = true;
    public bool EnableRateLimiting { get; set; } = true;
    public int RateLimitRequestsPerMinute { get; set; } = 5;
}
