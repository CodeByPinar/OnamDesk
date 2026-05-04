using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Models;
using OnamDesk.Services;
using System.Collections.ObjectModel;

namespace OnamDesk.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardService _dashboardService;
        private readonly SettingsService _settingsService;

        public DashboardViewModel()
        {
            _dashboardService = new DashboardService();
            _settingsService = new SettingsService();

            LatestConsentForms = new ObservableCollection<ConsentForm>();
            LatestAuditLogs = new ObservableCollection<AuditLog>();

            _ = LoadDashboardAsync();
        }

        public ObservableCollection<ConsentForm> LatestConsentForms { get; }

        public ObservableCollection<AuditLog> LatestAuditLogs { get; }

        [ObservableProperty]
        private string clinicName = "Demo Klinik";

        [ObservableProperty]
        private int totalPatients;

        [ObservableProperty]
        private int totalTemplates;

        [ObservableProperty]
        private int activeTemplates;

        [ObservableProperty]
        private int totalConsentForms;

        [ObservableProperty]
        private int pdfGeneratedCount;

        [ObservableProperty]
        private int totalAuditLogs;

        [ObservableProperty]
        private string statusMessage = "Dashboard hazır.";

        [RelayCommand]
        private async Task LoadDashboardAsync()
        {
            LatestConsentForms.Clear();
            LatestAuditLogs.Clear();

            var settings = await _settingsService.GetSettingsAsync();

            ClinicName = string.IsNullOrWhiteSpace(settings.ClinicName)
                ? "Demo Klinik"
                : settings.ClinicName.Trim();

            var summary = await _dashboardService.GetSummaryAsync();

            TotalPatients = summary.TotalPatients;
            TotalTemplates = summary.TotalTemplates;
            ActiveTemplates = summary.ActiveTemplates;
            TotalConsentForms = summary.TotalConsentForms;
            PdfGeneratedCount = summary.PdfGeneratedCount;
            TotalAuditLogs = summary.TotalAuditLogs;

            foreach (var consentForm in summary.LatestConsentForms)
            {
                LatestConsentForms.Add(consentForm);
            }

            foreach (var auditLog in summary.LatestAuditLogs)
            {
                LatestAuditLogs.Add(auditLog);
            }

            StatusMessage = "Dashboard verileri güncellendi.";
        }
    }
}