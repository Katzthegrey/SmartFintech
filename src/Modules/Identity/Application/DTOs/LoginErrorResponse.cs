using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs
{
    public class LoginErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public bool? Locked { get; set; }
        public int? UnlockAfterMinutes { get; set; }
        public int? FailedAttempts { get; set; }
    }
}
