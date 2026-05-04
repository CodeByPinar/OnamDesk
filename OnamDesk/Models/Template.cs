using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnamDesk.Models
{
    public class Template
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ContentJson { get; set; } = string.Empty;

        public string Risks { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConsentForm> ConsentForms { get; set; } = new List<ConsentForm>();
    }
}
