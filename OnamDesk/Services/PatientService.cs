using Microsoft.EntityFrameworkCore;
using OnamDesk.Data;
using OnamDesk.Models;

namespace OnamDesk.Services
{
    public class PatientService
    {
        public async Task<List<Patient>> GetAllAsync()
        {
            using var context = new AppDbContext();

            return await context.Patients
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            using var context = new AppDbContext();

            return await context.Patients
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Patient>> SearchAsync(string searchText)
        {
            using var context = new AppDbContext();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync();
            }

            searchText = searchText.Trim().ToLower();

            return await context.Patients
                .Where(x => x.FullName.ToLower().Contains(searchText)
                            || (x.Phone != null && x.Phone.Contains(searchText)))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Patient patient)
        {
            using var context = new AppDbContext();

            patient.CreatedAt = DateTime.UtcNow;

            await context.Patients.AddAsync(patient);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Patient patient)
        {
            using var context = new AppDbContext();

            var existingPatient = await context.Patients
                .FirstOrDefaultAsync(x => x.Id == patient.Id);

            if (existingPatient is null)
            {
                return;
            }

            existingPatient.FullName = patient.FullName;
            existingPatient.TcNoEncrypted = patient.TcNoEncrypted;
            existingPatient.BirthDate = patient.BirthDate;
            existingPatient.Phone = patient.Phone;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = new AppDbContext();

            var patient = await context.Patients
                .FirstOrDefaultAsync(x => x.Id == id);

            if (patient is null)
            {
                return;
            }

            context.Patients.Remove(patient);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByTcNoEncryptedAsync(string tcNoEncrypted, int? excludePatientId = null)
        {
            using var context = new AppDbContext();

            var query = context.Patients
                .Where(x => x.TcNoEncrypted == tcNoEncrypted);

            if (excludePatientId.HasValue)
            {
                query = query.Where(x => x.Id != excludePatientId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasConsentFormsAsync(int patientId)
        {
            using var context = new AppDbContext();

            return await context.ConsentForms
                .AnyAsync(x => x.PatientId == patientId);
        }
    }
}