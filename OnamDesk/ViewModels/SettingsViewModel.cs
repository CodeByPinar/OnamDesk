using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OnamDesk.Helpers;
using OnamDesk.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace OnamDesk.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly AuditLogService _auditLogService;
        private readonly AuthService _authService;
        private readonly BackupService _backupService;

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();
            _auditLogService = new AuditLogService();
            _authService = new AuthService();
            _backupService = new BackupService();

            BackupFiles = new ObservableCollection<string>();

            _ = LoadSettingsAsync();
            LoadBackupFiles();
        }

        [ObservableProperty]
        private string clinicName = string.Empty;

        [ObservableProperty]
        private string defaultDoctorName = string.Empty;

        [ObservableProperty]
        private string archiveFolderPath = string.Empty;

        [ObservableProperty]
        private string pdfHeaderTitle = string.Empty;

        [ObservableProperty]
        private string currentUserName = string.Empty;

        [ObservableProperty]
        private string statusMessage = "Ayarlar ekranı hazır.";

        [ObservableProperty]
        private string currentPassword = string.Empty;

        [ObservableProperty]
        private string newPassword = string.Empty;

        [ObservableProperty]
        private string confirmNewPassword = string.Empty;

        public ObservableCollection<string> BackupFiles { get; }

        [ObservableProperty]
        private string? selectedBackupFile;

        [RelayCommand]
        private async Task LoadSettingsAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            ClinicName = settings.ClinicName;
            DefaultDoctorName = settings.DefaultDoctorName;
            ArchiveFolderPath = settings.ArchiveFolderPath;
            PdfHeaderTitle = settings.PdfHeaderTitle;
            CurrentUserName = settings.CurrentUserName;

            StatusMessage = "Ayarlar yüklendi.";
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            if (!ValidateSettings())
            {
                return;
            }

            var settings = new AppSettings
            {
                ClinicName = ClinicName.Trim(),
                DefaultDoctorName = DefaultDoctorName.Trim(),
                ArchiveFolderPath = ArchiveFolderPath.Trim(),
                PdfHeaderTitle = PdfHeaderTitle.Trim(),
                CurrentUserName = CurrentUserName.Trim()
            };

            await _settingsService.SaveSettingsAsync(settings);

            await _auditLogService.AddAsync(
                action: "SETTINGS_UPDATED",
                userName: CurrentUserName,
                details: new
                {
                    ClinicName = settings.ClinicName,
                    DefaultDoctorName = settings.DefaultDoctorName,
                    ArchiveFolderPath = settings.ArchiveFolderPath,
                    PdfHeaderTitle = settings.PdfHeaderTitle,
                    UpdatedAt = DateTime.UtcNow
                });

            WeakReferenceMessenger.Default.Send(new SettingsUpdatedMessage());

            StatusMessage = "Ayarlar başarıyla kaydedildi.";
        }

        [RelayCommand]
        private async Task ResetSettingsAsync()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var defaultArchiveFolder = Path.Combine(documentsPath, "OnamDesk", "Archive");

            ClinicName = "Demo Klinik";
            DefaultDoctorName = "Demo Doktor";
            ArchiveFolderPath = defaultArchiveFolder;
            PdfHeaderTitle = "ONAM FORMU";
            CurrentUserName = "Admin";

            await SaveSettingsAsync();

            StatusMessage = "Ayarlar varsayılana döndürüldü.";
        }

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                StatusMessage = "Mevcut şifre zorunludur.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                StatusMessage = "Yeni şifre zorunludur.";
                return;
            }

            if (NewPassword.Length < 6)
            {
                StatusMessage = "Yeni şifre en az 6 karakter olmalıdır.";
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                StatusMessage = "Yeni şifre ve tekrar alanı eşleşmiyor.";
                return;
            }

            try
            {
                await _authService.ChangePasswordAsync(CurrentPassword, NewPassword);

                await _auditLogService.AddAsync(
                    action: "PASSWORD_CHANGED",
                    userName: string.IsNullOrWhiteSpace(CurrentUserName) ? "Admin" : CurrentUserName.Trim(),
                    details: new
                    {
                        ChangedAt = DateTime.UtcNow
                    });

                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;

                StatusMessage = "Şifre başarıyla değiştirildi.";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private void LoadBackupFiles()
        {
            BackupFiles.Clear();

            var files = _backupService.GetBackupFiles();

            foreach (var file in files)
            {
                BackupFiles.Add(file);
            }

            StatusMessage = $"{BackupFiles.Count} yedek dosyası listelendi.";
        }

        [RelayCommand]
        private void OpenSelectedBackupFolder()
        {
            if (string.IsNullOrWhiteSpace(SelectedBackupFile) || !File.Exists(SelectedBackupFile))
            {
                StatusMessage = "Açılacak yedek dosyası seçilmedi.";
                return;
            }

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{SelectedBackupFile}\"",
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);

                StatusMessage = "Yedek dosyası klasörde gösterildi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Yedek klasörü açılırken hata oluştu: {ex.Message}";
            }
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(ClinicName))
            {
                StatusMessage = "Klinik adı zorunludur.";
                return false;
            }

            if (ClinicName.Trim().Length < 3)
            {
                StatusMessage = "Klinik adı en az 3 karakter olmalıdır.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DefaultDoctorName))
            {
                StatusMessage = "Varsayılan doktor adı zorunludur.";
                return false;
            }

            if (DefaultDoctorName.Trim().Length < 3)
            {
                StatusMessage = "Varsayılan doktor adı en az 3 karakter olmalıdır.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ArchiveFolderPath))
            {
                StatusMessage = "Arşiv klasörü zorunludur.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(PdfHeaderTitle))
            {
                StatusMessage = "PDF başlığı zorunludur.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentUserName))
            {
                StatusMessage = "Aktif kullanıcı adı zorunludur.";
                return false;
            }

            return true;
        }
    }
}