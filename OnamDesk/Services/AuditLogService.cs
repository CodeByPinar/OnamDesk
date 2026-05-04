using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OnamDesk.Data;
using OnamDesk.Models;

namespace OnamDesk.Services
{
    public class AuditLogService
    {
        public async Task AddAsync(
            string action,
            string userName,
            int? consentId = null,
            object? details = null)
        {
            using var context = new AppDbContext();

            var auditLog = new AuditLog
            {
                ConsentId = consentId,
                Action = action,
                UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName.Trim(),
                Timestamp = DateTime.UtcNow,
                Details = details is null
                    ? null
                    : JsonConvert.SerializeObject(details)
            };

            await context.AuditLogs.AddAsync(auditLog);
            await context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            using var context = new AppDbContext();

            return await context.AuditLogs
                .Include(x => x.ConsentForm)
                .ThenInclude(x => x!.Patient)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByConsentIdAsync(int consentId)
        {
            using var context = new AppDbContext();

            return await context.AuditLogs
                .Where(x => x.ConsentId == consentId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> SearchAsync(string searchText)
        {
            using var context = new AppDbContext();

            var query = context.AuditLogs
                .Include(x => x.ConsentForm)
                .ThenInclude(x => x!.Patient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim().ToLower();

                query = query.Where(x =>
                    x.Action.ToLower().Contains(searchText) ||
                    x.UserName.ToLower().Contains(searchText) ||
                    (x.Details != null && x.Details.ToLower().Contains(searchText)) ||
                    (x.ConsentForm != null &&
                     x.ConsentForm.Patient != null &&
                     x.ConsentForm.Patient.FullName.ToLower().Contains(searchText)));
            }

            return await query
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }
    }
}