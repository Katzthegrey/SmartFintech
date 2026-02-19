using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string AssignedBy { get; set; } = "system";
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
