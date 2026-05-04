using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Models;
using OnamDesk.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace OnamDesk.ViewModels
{
    public partial class ArchiveViewModel : ViewModelBase
    {
        private readonly ConsentService _consentService;
        private readonly SignatureService _signatureService;
        private readonly PdfService _pdfService;
        private readonly AuditLogService _auditLogService;
        private readonly SettingsService _settingsService;

        public ArchiveViewModel()
        {
            _consentService = new ConsentService();
            _signatureService = new SignatureService();
            _pdfService = new PdfService();
            _auditLogService = new AuditLogService();
            _settingsService = new SettingsService();

            ConsentForms = new ObservableCollection<ConsentForm>();

            _ = LoadConsentFormsAsync();
        }

        public ObservableCollection<ConsentForm> ConsentForms { get; }

        [ObservableProperty]
        private ConsentForm? selectedConsentForm;

        [ObservableProperty]
        private string searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchConsentFormsAsync();
        }

        [ObservableProperty]
        private string statusMessage = "Arşiv ekranı hazır.";

        [RelayCommand]
        private async Task LoadConsentFormsAsync()
        {
            ConsentForms.Clear();

            var consentForms = await _consentService.GetAllAsync();

            foreach (var consentForm in consentForms)
            {
                ConsentForms.Add(consentForm);
            }

            StatusMessage = $"{ConsentForms.Count} onam kaydı listelendi.";
        }

        [RelayCommand]
        private async Task SearchConsentFormsAsync()
        {
            ConsentForms.Clear();

            var consentForms = await _consentService.SearchAsync(SearchText);

            foreach (var consentForm in consentForms)
            {
                ConsentForms.Add(consentForm);
            }

            StatusMessage = $"{ConsentForms.Count} sonuç bulundu.";
        }

        [RelayCommand]
        private void VerifySignature()
        {
            if (SelectedConsentForm is null)
            {
                StatusMessage = "Doğrulanacak onam kaydı seçilmedi.";
                return;
            }

            var calculatedHash = _signatureService.GenerateSignatureHash(
                SelectedConsentForm.SignatureData);

            var isValid = string.Equals(
                calculatedHash,
                SelectedConsentForm.SignatureHash,
                StringComparison.OrdinalIgnoreCase);

            StatusMessage = isValid
                ? "İmza hash doğrulaması başarılı. Kayıt bütünlüğü korunuyor."
                : $"İmza hash doğrulaması başarısız. Beklenen: {SelectedConsentForm.SignatureHash[..Math.Min(12, SelectedConsentForm.SignatureHash.Length)]}... Hesaplanan: {calculatedHash[..Math.Min(12, calculatedHash.Length)]}...";
        }

        [RelayCommand]
        private async Task GeneratePdfAsync()
        {
            if (SelectedConsentForm is null)
            {
                StatusMessage = "PDF oluşturmak için bir onam kaydı seçin.";
                return;
            }

            try
            {
                var fullConsentForm = await _consentService.GetByIdAsync(SelectedConsentForm.Id);

                if (fullConsentForm is null)
                {
                    StatusMessage = "Onam kaydı bulunamadı.";
                    return;
                }

                var pdfPath = await _pdfService.GenerateConsentPdfAsync(fullConsentForm);

                await _consentService.UpdatePdfPathAsync(fullConsentForm.Id, pdfPath);

                var auditUserName = await GetAuditUserNameAsync();

                await _auditLogService.AddAsync(
                    action: "PDF_CREATED",
                    userName: auditUserName,
                    consentId: fullConsentForm.Id,
                    details: new
                    {
                        PdfPath = pdfPath,
                        PatientName = fullConsentForm.Patient?.FullName,
                        TemplateName = fullConsentForm.Template?.Name,
                        CreatedAt = DateTime.UtcNow
                    });

                await LoadConsentFormsAsync();

                SelectedConsentForm = ConsentForms.FirstOrDefault(x => x.Id == fullConsentForm.Id);

                StatusMessage = $"PDF başarıyla oluşturuldu: {pdfPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"PDF oluşturulurken hata oluştu: {ex.Message}";
            }
        }

        [RelayCommand]
        private void OpenPdf()
        {
            if (SelectedConsentForm is null)
            {
                StatusMessage = "PDF açmak için bir onam kaydı seçin.";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedConsentForm.PdfPath))
            {
                StatusMessage = "Bu onam kaydı için henüz PDF oluşturulmamış.";
                return;
            }

            try
            {
                _pdfService.OpenPdf(SelectedConsentForm.PdfPath);

                StatusMessage = "PDF dosyası açıldı.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"PDF açılırken hata oluştu: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task DeleteConsentFormAsync()
        {
            if (SelectedConsentForm is null)
            {
                StatusMessage = "Silinecek onam kaydı seçilmedi.";
                return;
            }

            var selectedConsentId = SelectedConsentForm.Id;
            var patientName = SelectedConsentForm.Patient?.FullName;
            var templateName = SelectedConsentForm.Template?.Name;
            var doctorName = SelectedConsentForm.DoctorName;
            var signedAt = SelectedConsentForm.SignedAt;
            var pdfPath = SelectedConsentForm.PdfPath;

            var result = MessageBox.Show(
                $"{patientName} adlı hastaya ait onam kaydını silmek istediğinize emin misiniz?",
                "Onam Kaydı Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Silme işlemi iptal edildi.";
                return;
            }

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "CONSENT_DELETED",
                userName: auditUserName,
                consentId: selectedConsentId,
                details: new
                {
                    PatientName = patientName,
                    TemplateName = templateName,
                    DoctorName = doctorName,
                    SignedAt = signedAt,
                    PdfPath = pdfPath,
                    DeletedAt = DateTime.UtcNow
                });

            await _consentService.DeleteAsync(selectedConsentId);

            SelectedConsentForm = null;

            await LoadConsentFormsAsync();

            StatusMessage = "Onam kaydı başarıyla silindi.";
        }

        private async Task<string> GetAuditUserNameAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            return string.IsNullOrWhiteSpace(settings.CurrentUserName)
                ? "Admin"
                : settings.CurrentUserName.Trim();
        }
    }
}