using SmartFintechFinancial.Shared.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities
{
    public class FailedLoginAttempt : IAuditableEntity
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }
        public virtual User? User { get; set;  } 
        public string? Email { get; set; } 
        public string IpAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public string UserAgent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = "system";

        // Optional: Add method to check if attempt is recent
        public bool IsRecent(TimeSpan timeWindow)
        {
            // Don't consider future dates as recent
            if (CreatedAt > DateTime.UtcNow)
                return false;

            return DateTime.UtcNow - CreatedAt < timeWindow;
        }
    }
}
