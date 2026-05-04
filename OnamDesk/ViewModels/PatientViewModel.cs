using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Models;
using OnamDesk.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace OnamDesk.ViewModels
{
    public partial class PatientViewModel : ViewModelBase
    {
        private readonly PatientService _patientService;
        private readonly EncryptionService _encryptionService;
        private readonly AuditLogService _auditLogService;
        private readonly SettingsService _settingsService;

        public PatientViewModel()
        {
            _patientService = new PatientService();
            _encryptionService = new EncryptionService();
            _auditLogService = new AuditLogService();
            _settingsService = new SettingsService();

            Patients = new ObservableCollection<Patient>();

            BirthDate = DateTime.Today.AddYears(-30);

            _ = LoadPatientsAsync();
        }

        public ObservableCollection<Patient> Patients { get; }

        [ObservableProperty]
        private Patient? selectedPatient;

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string tcNo = string.Empty;

        [ObservableProperty]
        private DateTime birthDate;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchPatientsAsync();
        }

        [ObservableProperty]
        private string statusMessage = "Hasta kayıt ekranı hazır.";

        partial void OnSelectedPatientChanged(Patient? value)
        {
            if (value is null)
            {
                return;
            }

            FullName = value.FullName;
            TcNo = _encryptionService.Decrypt(value.TcNoEncrypted);
            BirthDate = value.BirthDate;
            Phone = value.Phone ?? string.Empty;
        }

        [RelayCommand]
        private async Task LoadPatientsAsync()
        {
            Patients.Clear();

            var patients = await _patientService.GetAllAsync();

            foreach (var patient in patients)
            {
                Patients.Add(patient);
            }

            StatusMessage = $"{Patients.Count} hasta kaydı listelendi.";
        }

        [RelayCommand]
        private async Task SearchPatientsAsync()
        {
            Patients.Clear();

            var patients = await _patientService.SearchAsync(SearchText);

            foreach (var patient in patients)
            {
                Patients.Add(patient);
            }

            StatusMessage = $"{Patients.Count} sonuç bulundu.";
        }

        [RelayCommand]
        private async Task AddPatientAsync()
        {
            if (!ValidatePatientForm())
            {
                return;
            }

            var encryptedTcNo = _encryptionService.Encrypt(TcNo.Trim());

            var exists = await _patientService.ExistsByTcNoEncryptedAsync(encryptedTcNo);

            if (exists)
            {
                StatusMessage = "Bu TC kimlik numarasıyla kayıtlı bir hasta zaten var.";
                return;
            }

            var patient = new Patient
            {
                FullName = FullName.Trim(),
                TcNoEncrypted = encryptedTcNo,
                BirthDate = BirthDate,
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim()
            };

            await _patientService.AddAsync(patient);

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "PATIENT_CREATED",
                userName: auditUserName,
                details: new
                {
                    PatientId = patient.Id,
                    PatientName = patient.FullName,
                    BirthDate = patient.BirthDate,
                    Phone = patient.Phone,
                    CreatedAt = DateTime.UtcNow
                });

            ClearForm();

            await LoadPatientsAsync();

            StatusMessage = "Hasta başarıyla eklendi.";
        }

        [RelayCommand]
        private async Task UpdatePatientAsync()
        {
            if (SelectedPatient is null)
            {
                StatusMessage = "Güncellenecek hasta seçilmedi.";
                return;
            }

            if (!ValidatePatientForm())
            {
                return;
            }

            var selectedPatientId = SelectedPatient.Id;
            var oldFullName = SelectedPatient.FullName;
            var oldBirthDate = SelectedPatient.BirthDate;
            var oldPhone = SelectedPatient.Phone;

            var encryptedTcNo = _encryptionService.Encrypt(TcNo.Trim());

            var exists = await _patientService.ExistsByTcNoEncryptedAsync(encryptedTcNo, selectedPatientId);

            if (exists)
            {
                StatusMessage = "Bu TC kimlik numarası başka bir hastada kayıtlı.";
                return;
            }

            SelectedPatient.FullName = FullName.Trim();
            SelectedPatient.TcNoEncrypted = encryptedTcNo;
            SelectedPatient.BirthDate = BirthDate;
            SelectedPatient.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();

            await _patientService.UpdateAsync(SelectedPatient);

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "PATIENT_UPDATED",
                userName: auditUserName,
                details: new
                {
                    PatientId = selectedPatientId,
                    OldFullName = oldFullName,
                    NewFullName = SelectedPatient.FullName,
                    OldBirthDate = oldBirthDate,
                    NewBirthDate = SelectedPatient.BirthDate,
                    OldPhone = oldPhone,
                    NewPhone = SelectedPatient.Phone,
                    UpdatedAt = DateTime.UtcNow
                });

            await LoadPatientsAsync();

            SelectedPatient = Patients.FirstOrDefault(x => x.Id == selectedPatientId);

            StatusMessage = "Hasta başarıyla güncellendi.";
        }

        [RelayCommand]
        private async Task DeletePatientAsync()
        {
            if (SelectedPatient is null)
            {
                StatusMessage = "Silinecek hasta seçilmedi.";
                return;
            }

            var hasConsentForms = await _patientService.HasConsentFormsAsync(SelectedPatient.Id);

            if (hasConsentForms)
            {
                StatusMessage = "Bu hastaya ait onam formu olduğu için hasta silinemez.";
                return;
            }

            var selectedPatientId = SelectedPatient.Id;
            var patientName = SelectedPatient.FullName;
            var birthDate = SelectedPatient.BirthDate;
            var phone = SelectedPatient.Phone;

            var result = MessageBox.Show(
                $"{patientName} adlı hastayı silmek istediğinize emin misiniz?",
                "Hasta Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Silme işlemi iptal edildi.";
                return;
            }

            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "PATIENT_DELETED",
                userName: auditUserName,
                details: new
                {
                    PatientId = selectedPatientId,
                    PatientName = patientName,
                    BirthDate = birthDate,
                    Phone = phone,
                    DeletedAt = DateTime.UtcNow
                });

            await _patientService.DeleteAsync(selectedPatientId);

            ClearForm();

            await LoadPatientsAsync();

            StatusMessage = "Hasta başarıyla silindi.";
        }

        private bool ValidatePatientForm()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                StatusMessage = "Hasta adı soyadı zorunludur.";
                return false;
            }

            if (FullName.Trim().Length < 3)
            {
                StatusMessage = "Hasta adı soyadı en az 3 karakter olmalıdır.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TcNo))
            {
                StatusMessage = "TC kimlik numarası zorunludur.";
                return false;
            }

            if (TcNo.Trim().Length != 11)
            {
                StatusMessage = "TC kimlik numarası 11 haneli olmalıdır.";
                return false;
            }

            if (!TcNo.Trim().All(char.IsDigit))
            {
                StatusMessage = "TC kimlik numarası sadece rakamlardan oluşmalıdır.";
                return false;
            }

            if (BirthDate > DateTime.Today)
            {
                StatusMessage = "Doğum tarihi bugünden ileri olamaz.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Phone))
            {
                var cleanedPhone = Phone.Trim()
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "");

                if (!cleanedPhone.All(char.IsDigit))
                {
                    StatusMessage = "Telefon numarası sadece rakam, boşluk, tire veya parantez içerebilir.";
                    return false;
                }

                if (cleanedPhone.Length < 10 || cleanedPhone.Length > 11)
                {
                    StatusMessage = "Telefon numarası 10 veya 11 haneli olmalıdır.";
                    return false;
                }
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
            SelectedPatient = null;
            FullName = string.Empty;
            TcNo = string.Empty;
            BirthDate = DateTime.Today.AddYears(-30);
            Phone = string.Empty;
            StatusMessage = "Form temizlendi.";
        }
    }
}
