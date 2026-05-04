using Microsoft.EntityFrameworkCore;
using OnamDesk.Data;
using OnamDesk.Models;

namespace OnamDesk.Services
{
    public class ConsentService
    {
        public async Task<List<ConsentForm>> GetAllAsync()
        {
            using var context = new AppDbContext();

            return await context.ConsentForms
                .Include(x => x.Patient)
                .Include(x => x.Template)
                .OrderByDescending(x => x.SignedAt)
                .ToListAsync();
        }

        public async Task<ConsentForm?> GetByIdAsync(int id)
        {
            using var context = new AppDbContext();

            return await context.ConsentForms
                .Include(x => x.Patient)
                .Include(x => x.Template)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ConsentForm>> GetByPatientIdAsync(int patientId)
        {
            using var context = new AppDbContext();

            return await context.ConsentForms
                .Include(x => x.Patient)
                .Include(x => x.Template)
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.SignedAt)
                .ToListAsync();
        }

        public async Task<List<ConsentForm>> SearchAsync(string searchText)
        {
            using var context = new AppDbContext();

            var query = context.ConsentForms
                .Include(x => x.Patient)
                .Include(x => x.Template)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim().ToLower();

                query = query.Where(x =>
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(searchText)) ||
                    (x.Template != null && x.Template.Name.ToLower().Contains(searchText)) ||
                    x.DoctorName.ToLower().Contains(searchText));
            }

            return await query
                .OrderByDescending(x => x.SignedAt)
                .ToListAsync();
        }

        public async Task AddAsync(ConsentForm consentForm)
        {
            using var context = new AppDbContext();

            if (consentForm.SignedAt == default)
            {
                consentForm.SignedAt = DateTime.UtcNow;
            }

            await context.ConsentForms.AddAsync(consentForm);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePdfPathAsync(int consentFormId, string pdfPath)
        {
            using var context = new AppDbContext();

            var consentForm = await context.ConsentForms
                .FirstOrDefaultAsync(x => x.Id == consentFormId);

            if (consentForm is null)
            {
                return;
            }

            consentForm.PdfPath = pdfPath;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = new AppDbContext();

            var consentForm = await context.ConsentForms
                .FirstOrDefaultAsync(x => x.Id == id);

            if (consentForm is null)
            {
                return;
            }

            context.ConsentForms.Remove(consentForm);
            await context.SaveChangesAsync();
        }
    }
}