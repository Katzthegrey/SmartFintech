using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs;

public record LoginRequest(
    string Email,
    string Password,
    string? TwoFactorCode = null, 
    bool RememberMe = false);
