using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnamDesk.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string TcNoEncrypted { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ConsentForm> ConsentForms { get; set; } = new List<ConsentForm>();

    }
}
