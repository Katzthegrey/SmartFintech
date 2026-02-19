using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFintechFinancial.Modules.Identity.Domain.Entities
{
    public class LoginLog
    {
        public Guid Id { get; set; }

  
        public Guid UserId { get; set; }


        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsSuccess { get; set; }

        [MaxLength(50)]
        public string? FailureReason { get; set; } // Only populated if Success = false

        [MaxLength(255)]
        public string? Location { get; set; } // Could be populated from IP geolocation

        [MaxLength(50)]
        public string? DeviceType { get; set; } // e.g., "Mobile", "Desktop", "Tablet"

        public bool TwoFactorUsed { get; set; }
        public string CreatedBy { get; set; } = "system";
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
