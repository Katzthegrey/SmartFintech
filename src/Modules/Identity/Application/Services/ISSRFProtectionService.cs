using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.Services
{
    public interface ISSRFProtectionService
    {
        bool IsValidUrl(string url);
        bool IsInternalIpAddress(string host);
        bool IsAllowedDomain(string domain);
    }
}
