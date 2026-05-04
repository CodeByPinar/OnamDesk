using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnamDesk.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? ConsentId { get; set; }

        public ConsentForm? ConsentForm { get; set; }

        public string Action { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Details { get; set; }
    }
}
