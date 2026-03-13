using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Application.DTOs
{
    public record RevokeTokenRequest
    (
        string RefreshToken
        );
}
