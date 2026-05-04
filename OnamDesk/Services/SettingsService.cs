using Newtonsoft.Json;
using System.IO;

namespace OnamDesk.Services
{
    public class SettingsService
    {
        private readonly string _settingsFolder;
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            _settingsFolder = Path.Combine(appDataPath, "OnamDesk");
            _settingsFilePath = Path.Combine(_settingsFolder, "settings.json");

            if (!Directory.Exists(_settingsFolder))
            {
                Directory.CreateDirectory(_settingsFolder);
            }
        }

        public async Task<AppSettings> GetSettingsAsync()
        {
            if (!File.Exists(_settingsFilePath))
            {
                var defaultSettings = CreateDefaultSettings();

                await SaveSettingsAsync(defaultSettings);

                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_settingsFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                var defaultSettings = CreateDefaultSettings();

                await SaveSettingsAsync(defaultSettings);

                return defaultSettings;
            }

            var settings = JsonConvert.DeserializeObject<AppSettings>(json);

            return settings ?? CreateDefaultSettings();
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            if (!Directory.Exists(_settingsFolder))
            {
                Directory.CreateDirectory(_settingsFolder);
            }

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        private static AppSettings CreateDefaultSettings()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var defaultArchiveFolder = Path.Combine(documentsPath, "OnamDesk", "Archive");

            return new AppSettings
            {
                ClinicName = "Demo Klinik",
                DefaultDoctorName = "Demo Doktor",
                ArchiveFolderPath = defaultArchiveFolder,
                PdfHeaderTitle = "ONAM FORMU",
                CurrentUserName = "Admin"
            };
        }
    }

    public class AppSettings
    {
        public string ClinicName { get; set; } = string.Empty;

        public string DefaultDoctorName { get; set; } = string.Empty;

        public string ArchiveFolderPath { get; set; } = string.Empty;

        public string PdfHeaderTitle { get; set; } = string.Empty;

        public string CurrentUserName { get; set; } = string.Empty;
    }
}