using Microsoft.EntityFrameworkCore;
using OnamDesk.Data;
using OnamDesk.Models;

namespace OnamDesk.Services
{
    public class TemplateService
    {
        public async Task<List<Template>> GetAllAsync()
        {
            using var context = new AppDbContext();

            return await context.Templates
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<Template>> GetActiveAsync()
        {
            using var context = new AppDbContext();

            return await context.Templates
                .Where(x => x.IsActive)
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Template?> GetByIdAsync(int id)
        {
            using var context = new AppDbContext();

            return await context.Templates
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Template>> SearchAsync(string searchText)
        {
            using var context = new AppDbContext();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync();
            }

            searchText = searchText.Trim().ToLower();

            return await context.Templates
                .Where(x =>
                    x.Name.ToLower().Contains(searchText) ||
                    x.Category.ToLower().Contains(searchText) ||
                    x.Risks.ToLower().Contains(searchText))
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Template template)
        {
            using var context = new AppDbContext();

            template.UpdatedAt = DateTime.UtcNow;

            await context.Templates.AddAsync(template);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Template template)
        {
            using var context = new AppDbContext();

            var existingTemplate = await context.Templates
                .FirstOrDefaultAsync(x => x.Id == template.Id);

            if (existingTemplate is null)
            {
                return;
            }

            existingTemplate.Name = template.Name;
            existingTemplate.Category = template.Category;
            existingTemplate.ContentJson = template.ContentJson;
            existingTemplate.Risks = template.Risks;
            existingTemplate.IsActive = template.IsActive;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var context = new AppDbContext();

            var template = await context.Templates
                .FirstOrDefaultAsync(x => x.Id == id);

            if (template is null)
            {
                return;
            }

            context.Templates.Remove(template);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeTemplateId = null)
        {
            using var context = new AppDbContext();

            var normalizedName = name.Trim().ToLower();

            var query = context.Templates
                .Where(x => x.Name.ToLower() == normalizedName);

            if (excludeTemplateId.HasValue)
            {
                query = query.Where(x => x.Id != excludeTemplateId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasConsentFormsAsync(int templateId)
        {
            using var context = new AppDbContext();

            return await context.ConsentForms
                .AnyAsync(x => x.TemplateId == templateId);
        }
    }
}