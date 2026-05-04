using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnamDesk.Models
{
    public class ConsentForm
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public Patient? Patient { get; set; }

        public int TemplateId { get; set; }

        public Template? Template { get; set; }

        public string SignatureData { get; set; } = string.Empty;

        public string SignatureHash { get; set; } = string.Empty;

        public DateTime SignedAt { get; set; } = DateTime.UtcNow;

        public string PdfPath { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
