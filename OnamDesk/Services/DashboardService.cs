using Microsoft.EntityFrameworkCore;
using OnamDesk.Data;
using OnamDesk.Models;

namespace OnamDesk.Services
{
    public class DashboardService
    {
        public async Task<DashboardSummary> GetSummaryAsync()
        {
            using var context = new AppDbContext();

            var totalPatients = await context.Patients.CountAsync();
            var totalTemplates = await context.Templates.CountAsync();
            var activeTemplates = await context.Templates.CountAsync(x => x.IsActive);
            var totalConsentForms = await context.ConsentForms.CountAsync();
            var pdfGeneratedCount = await context.ConsentForms.CountAsync(x => !string.IsNullOrWhiteSpace(x.PdfPath));
            var totalAuditLogs = await context.AuditLogs.CountAsync();

            var latestConsentForms = await context.ConsentForms
                .Include(x => x.Patient)
                .Include(x => x.Template)
                .OrderByDescending(x => x.SignedAt)
                .Take(6)
                .ToListAsync();

            var latestAuditLogs = await context.AuditLogs
                .Include(x => x.ConsentForm)
                .ThenInclude(x => x!.Patient)
                .OrderByDescending(x => x.Timestamp)
                .Take(6)
                .ToListAsync();

            return new DashboardSummary
            {
                TotalPatients = totalPatients,
                TotalTemplates = totalTemplates,
                ActiveTemplates = activeTemplates,
                TotalConsentForms = totalConsentForms,
                PdfGeneratedCount = pdfGeneratedCount,
                TotalAuditLogs = totalAuditLogs,
                LatestConsentForms = latestConsentForms,
                LatestAuditLogs = latestAuditLogs
            };
        }
    }

    public class DashboardSummary
    {
        public int TotalPatients { get; set; }

        public int TotalTemplates { get; set; }

        public int ActiveTemplates { get; set; }

        public int TotalConsentForms { get; set; }

        public int PdfGeneratedCount { get; set; }

        public int TotalAuditLogs { get; set; }

        public List<ConsentForm> LatestConsentForms { get; set; } = new();

        public List<AuditLog> LatestAuditLogs { get; set; } = new();
    }
}