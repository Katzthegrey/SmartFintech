using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.Services
{
    public interface IRateLimitingService
    {
        Task<bool> IsRateLimitedAsync(string endpoint, string identifier);
        Task RecordRequestAsync(string endpoint, string identifier);
        Task<int> GetRemainingRequestsAsync(string endpoint, string identifier);
    }
}
