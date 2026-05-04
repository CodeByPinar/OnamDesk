using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Models;
using OnamDesk.Services;
using System.Collections.ObjectModel;

namespace OnamDesk.ViewModels
{
    public partial class ConsentViewModel : ViewModelBase
    {
        private readonly PatientService _patientService;
        private readonly TemplateService _templateService;
        private readonly ConsentService _consentService;
        private readonly SignatureService _signatureService;
        private readonly AuditLogService _auditLogService;
        private readonly SettingsService _settingsService;

        public ConsentViewModel()
        {
            _patientService = new PatientService();
            _templateService = new TemplateService();
            _consentService = new ConsentService();
            _signatureService = new SignatureService();
            _auditLogService = new AuditLogService();
            _settingsService = new SettingsService();

            Patients = new ObservableCollection<Patient>();
            Templates = new ObservableCollection<Template>();

            _ = LoadInitialDataAsync();
        }

        public ObservableCollection<Patient> Patients { get; }

        public ObservableCollection<Template> Templates { get; }

        [ObservableProperty]
        private Patient? selectedPatient;

        [ObservableProperty]
        private Template? selectedTemplate;

        [ObservableProperty]
        private string doctorName = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        private string selectedTemplateContent = string.Empty;

        [ObservableProperty]
        private string selectedTemplateRisks = string.Empty;

        [ObservableProperty]
        private string statusMessage = "Onam formu ekranı hazır.";

        partial void OnSelectedTemplateChanged(Template? value)
        {
            if (value is null)
            {
                SelectedTemplateContent = string.Empty;
                SelectedTemplateRisks = string.Empty;
                return;
            }

            SelectedTemplateContent = value.ContentJson;
            SelectedTemplateRisks = value.Risks;
        }

        [RelayCommand]
        private async Task LoadInitialDataAsync()
        {
            Patients.Clear();
            Templates.Clear();

            await LoadDefaultDoctorNameAsync();

            var patients = await _patientService.GetAllAsync();
            var templates = await _templateService.GetActiveAsync();

            foreach (var patient in patients)
            {
                Patients.Add(patient);
            }

            foreach (var template in templates)
            {
                Templates.Add(template);
            }

            StatusMessage = $"{Patients.Count} hasta ve {Templates.Count} aktif şablon yüklendi.";
        }

        [RelayCommand]
        private async Task CreateTestConsentAsync()
        {
            if (!ValidateConsentForm())
            {
                return;
            }

            var signedAt = DateTime.UtcNow;

            var signatureData = _signatureService.CreateTestSignatureData(SelectedPatient!.FullName);
            var signatureHash = _signatureService.GenerateSignatureHash(signatureData);

            var consentForm = new ConsentForm
            {
                PatientId = SelectedPatient.Id,
                TemplateId = SelectedTemplate!.Id,
                SignatureData = signatureData,
                SignatureHash = signatureHash,
                SignedAt = signedAt,
                PdfPath = string.Empty,
                DoctorName = DoctorName.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
            };

            await _consentService.AddAsync(consentForm);

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "CONSENT_CREATED_TEST",
                userName: auditUserName,
                consentId: consentForm.Id,
                details: new
                {
                    PatientId = SelectedPatient.Id,
                    PatientName = SelectedPatient.FullName,
                    TemplateId = SelectedTemplate.Id,
                    TemplateName = SelectedTemplate.Name,
                    DoctorName = DoctorName.Trim(),
                    SignedAt = signedAt,
                    SignatureHash = signatureHash
                });

            StatusMessage = "Test onam formu başarıyla oluşturuldu.";

            ClearForm();

            await LoadInitialDataAsync();
        }

        public async Task CreateConsentWithSignatureAsync(string signatureData)
        {
            if (!ValidateConsentForm())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(signatureData))
            {
                StatusMessage = "Lütfen hasta imzası alın.";
                return;
            }

            var signedAt = DateTime.UtcNow;

            var signatureHash = _signatureService.GenerateSignatureHash(signatureData);

            var consentForm = new ConsentForm
            {
                PatientId = SelectedPatient!.Id,
                TemplateId = SelectedTemplate!.Id,
                SignatureData = signatureData,
                SignatureHash = signatureHash,
                SignedAt = signedAt,
                PdfPath = string.Empty,
                DoctorName = DoctorName.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
            };

            await _consentService.AddAsync(consentForm);

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "CONSENT_CREATED",
                userName: auditUserName,
                consentId: consentForm.Id,
                details: new
                {
                    PatientId = SelectedPatient.Id,
                    PatientName = SelectedPatient.FullName,
                    TemplateId = SelectedTemplate.Id,
                    TemplateName = SelectedTemplate.Name,
                    DoctorName = DoctorName.Trim(),
                    SignedAt = signedAt,
                    SignatureHash = signatureHash
                });

            StatusMessage = "İmzalı onam formu başarıyla oluşturuldu.";

            ClearForm();

            await LoadInitialDataAsync();
        }

        private bool ValidateConsentForm()
        {
            if (SelectedPatient is null)
            {
                StatusMessage = "Lütfen bir hasta seçin.";
                return false;
            }

            if (SelectedTemplate is null)
            {
                StatusMessage = "Lütfen bir onam şablonu seçin.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DoctorName))
            {
                StatusMessage = "Doktor adı zorunludur.";
                return false;
            }

            if (DoctorName.Trim().Length < 3)
            {
                StatusMessage = "Doktor adı en az 3 karakter olmalıdır.";
                return false;
            }

            return true;
        }

        private async Task LoadDefaultDoctorNameAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            DoctorName = string.IsNullOrWhiteSpace(settings.DefaultDoctorName)
                ? "Demo Doktor"
                : settings.DefaultDoctorName.Trim();
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
            SelectedPatient = null;
            SelectedTemplate = null;
            Notes = string.Empty;
            SelectedTemplateContent = string.Empty;
            SelectedTemplateRisks = string.Empty;
            StatusMessage = "Form temizlendi.";

            _ = LoadDefaultDoctorNameAsync();
        }
    }
}