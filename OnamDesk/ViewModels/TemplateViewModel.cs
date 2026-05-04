using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Models;
using OnamDesk.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace OnamDesk.ViewModels
{
    public partial class TemplateViewModel : ViewModelBase
    {
        private readonly TemplateService _templateService;
        private readonly AuditLogService _auditLogService;
        private readonly SettingsService _settingsService;

        public TemplateViewModel()
        {
            _templateService = new TemplateService();
            _auditLogService = new AuditLogService();
            _settingsService = new SettingsService();

            Templates = new ObservableCollection<Template>();

            IsActive = true;

            _ = LoadTemplatesAsync();
        }

        public ObservableCollection<Template> Templates { get; }

        [ObservableProperty]
        private Template? selectedTemplate;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string category = string.Empty;

        [ObservableProperty]
        private string contentJson = string.Empty;

        [ObservableProperty]
        private string risks = string.Empty;

        [ObservableProperty]
        private bool isActive = true;

        [ObservableProperty]
        private string searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchTemplatesAsync();
        }

        [ObservableProperty]
        private string statusMessage = "Şablon yönetimi hazır.";

        partial void OnSelectedTemplateChanged(Template? value)
        {
            if (value is null)
            {
                return;
            }

            Name = value.Name;
            Category = value.Category;
            ContentJson = value.ContentJson;
            Risks = value.Risks;
            IsActive = value.IsActive;
        }

        [RelayCommand]
        private async Task LoadTemplatesAsync()
        {
            Templates.Clear();

            var templates = await _templateService.GetAllAsync();

            foreach (var template in templates)
            {
                Templates.Add(template);
            }

            StatusMessage = $"{Templates.Count} şablon listelendi.";
        }

        [RelayCommand]
        private async Task SearchTemplatesAsync()
        {
            Templates.Clear();

            var templates = await _templateService.SearchAsync(SearchText);

            foreach (var template in templates)
            {
                Templates.Add(template);
            }

            StatusMessage = $"{Templates.Count} sonuç bulundu.";
        }

        [RelayCommand]
        private async Task AddTemplateAsync()
        {
            if (!ValidateTemplateForm())
            {
                return;
            }

            var exists = await _templateService.ExistsByNameAsync(Name);

            if (exists)
            {
                StatusMessage = "Bu isimde bir şablon zaten var.";
                return;
            }

            var template = new Template
            {
                Name = Name.Trim(),
                Category = Category.Trim(),
                ContentJson = ContentJson.Trim(),
                Risks = Risks.Trim(),
                IsActive = IsActive
            };

            await _templateService.AddAsync(template);

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "TEMPLATE_CREATED",
                userName: auditUserName,
                details: new
                {
                    TemplateId = template.Id,
                    TemplateName = template.Name,
                    Category = template.Category,
                    IsActive = template.IsActive,
                    CreatedAt = DateTime.UtcNow
                });

            ClearForm();

            await LoadTemplatesAsync();

            StatusMessage = "Şablon başarıyla eklendi.";
        }

        [RelayCommand]
        private async Task UpdateTemplateAsync()
        {
            if (SelectedTemplate is null)
            {
                StatusMessage = "Güncellenecek şablon seçilmedi.";
                return;
            }

            if (!ValidateTemplateForm())
            {
                return;
            }

            var selectedTemplateId = SelectedTemplate.Id;
            var oldName = SelectedTemplate.Name;
            var oldCategory = SelectedTemplate.Category;
            var oldIsActive = SelectedTemplate.IsActive;

            var exists = await _templateService.ExistsByNameAsync(Name, selectedTemplateId);

            if (exists)
            {
                StatusMessage = "Bu isimde başka bir şablon zaten var.";
                return;
            }

            SelectedTemplate.Name = Name.Trim();
            SelectedTemplate.Category = Category.Trim();
            SelectedTemplate.ContentJson = ContentJson.Trim();
            SelectedTemplate.Risks = Risks.Trim();
            SelectedTemplate.IsActive = IsActive;

            await _templateService.UpdateAsync(SelectedTemplate);

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "TEMPLATE_UPDATED",
                userName: auditUserName,
                details: new
                {
                    TemplateId = selectedTemplateId,
                    OldName = oldName,
                    NewName = SelectedTemplate.Name,
                    OldCategory = oldCategory,
                    NewCategory = SelectedTemplate.Category,
                    OldIsActive = oldIsActive,
                    NewIsActive = SelectedTemplate.IsActive,
                    UpdatedAt = DateTime.UtcNow
                });

            await LoadTemplatesAsync();

            SelectedTemplate = Templates.FirstOrDefault(x => x.Id == selectedTemplateId);

            StatusMessage = "Şablon başarıyla güncellendi.";
        }

        [RelayCommand]
        private async Task DeleteTemplateAsync()
        {
            if (SelectedTemplate is null)
            {
                StatusMessage = "Silinecek şablon seçilmedi.";
                return;
            }

            var hasConsentForms = await _templateService.HasConsentFormsAsync(SelectedTemplate.Id);

            if (hasConsentForms)
            {
                StatusMessage = "Bu şablona ait onam formu olduğu için şablon silinemez.";
                return;
            }

            var selectedTemplateId = SelectedTemplate.Id;
            var templateName = SelectedTemplate.Name;
            var category = SelectedTemplate.Category;
            var isActive = SelectedTemplate.IsActive;

            var result = MessageBox.Show(
                $"{templateName} adlı şablonu silmek istediğinize emin misiniz?",
                "Şablon Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Silme işlemi iptal edildi.";
                return;
            }

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "TEMPLATE_DELETED",
                userName: auditUserName,
                details: new
                {
                    TemplateId = selectedTemplateId,
                    TemplateName = templateName,
                    Category = category,
                    IsActive = isActive,
                    DeletedAt = DateTime.UtcNow
                });

            await _templateService.DeleteAsync(selectedTemplateId);

            ClearForm();

            await LoadTemplatesAsync();

            StatusMessage = "Şablon başarıyla silindi.";
        }

        private bool ValidateTemplateForm()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = "Şablon adı zorunludur.";
                return false;
            }

            if (Name.Trim().Length < 3)
            {
                StatusMessage = "Şablon adı en az 3 karakter olmalıdır.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                StatusMessage = "Kategori zorunludur.";
                return false;
            }

            if (Category.Trim().Length < 3)
            {
                StatusMessage = "Kategori en az 3 karakter olmalıdır.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ContentJson))
            {
                StatusMessage = "Açıklama / içerik alanı zorunludur.";
                return false;
            }

            if (ContentJson.Trim().Length < 10)
            {
                StatusMessage = "Açıklama / içerik alanı en az 10 karakter olmalıdır.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Risks))
            {
                StatusMessage = "Risk listesi zorunludur.";
                return false;
            }

            if (Risks.Trim().Length < 10)
            {
                StatusMessage = "Risk listesi en az 10 karakter olmalıdır.";
                return false;
            }

            return true;
        }

        private async Task<string> GetAuditUserNameAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            return string.IsNullOrWhiteSpace(settings.CurrentUserName)
                ? "Admin"
                : settings.CurrentUserName.Trim();
        }

        [RelayCommand]
        private void ClearForm()
        {
            SelectedTemplate = null;
            Name = string.Empty;
            Category = string.Empty;
            ContentJson = string.Empty;
            Risks = string.Empty;
            IsActive = true;
            StatusMessage = "Form temizlendi.";
        }
    }
}