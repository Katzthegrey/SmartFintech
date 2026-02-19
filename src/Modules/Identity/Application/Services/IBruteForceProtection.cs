using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.Services
{
    public interface IBruteForceProtection
    {
        Task<bool> IsAccountLockedAsync(string email);
        Task RecordFailedAttemptAsync(string email, string ipAddress);
        Task ResetFailedAttemptsAsync(string email);
        Task<int> GetFailedAttemptsCountAsync(string email);
    }
}
