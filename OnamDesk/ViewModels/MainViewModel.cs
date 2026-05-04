using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OnamDesk.Helpers;
using OnamDesk.Services;
using OnamDesk.Views;
using System.Windows.Controls;

namespace OnamDesk.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        public MainViewModel()
        {
            _settingsService = new SettingsService();

            CurrentView = new DashboardView();

            _ = LoadSettingsAsync();

            WeakReferenceMessenger.Default.Register<SettingsUpdatedMessage>(
            this,
            async (recipient, message) =>
            {
                await LoadSettingsAsync();
            });
        }

        public string ApplicationTitle { get; } = "OnamDesk";

        public event Action? LogoutRequested;

        [ObservableProperty]
        private string clinicName = "Demo Klinik";

        [ObservableProperty]
        private string currentUserName = "Admin";

        [ObservableProperty]
        private UserControl currentView;

        private async Task LoadSettingsAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();

            ClinicName = settings.ClinicName;
            CurrentUserName = settings.CurrentUserName;
        }

        [RelayCommand]
        private async Task RefreshSettingsAsync()
        {
            await LoadSettingsAsync();
        }

        [RelayCommand]
        private void ShowPatientView()
        {
            CurrentView = new PatientView();
        }

        [RelayCommand]
        private void ShowConsentView()
        {
            CurrentView = new ConsentView();
        }

        [RelayCommand]
        private void ShowDashboardView()
        {
            CurrentView = new DashboardView();
        }

        [RelayCommand]
        private void ShowArchiveView()
        {
            CurrentView = new ArchiveView();
        }

        [RelayCommand]
        private void ShowAuditLogView()
        {
            CurrentView = new AuditLogView();
        }

        [RelayCommand]
        private void ShowSettingsView()
        {
            CurrentView = new SettingsView();
        }

        [RelayCommand]
        private void Logout()
        {
            LogoutRequested?.Invoke();
        }

        [RelayCommand]
        private void ShowTemplateView()
        {
            CurrentView = new TemplateView();
        }
    }
}