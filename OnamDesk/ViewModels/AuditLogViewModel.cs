using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Models;
using OnamDesk.Services;
using System.Collections.ObjectModel;

namespace OnamDesk.ViewModels
{
    public partial class AuditLogViewModel : ViewModelBase
    {
        private readonly AuditLogService _auditLogService;

        public AuditLogViewModel()
        {
            _auditLogService = new AuditLogService();

            AuditLogs = new ObservableCollection<AuditLog>();

            _ = LoadAuditLogsAsync();
        }

        public ObservableCollection<AuditLog> AuditLogs { get; }

        [ObservableProperty]
        private AuditLog? selectedAuditLog;

        [ObservableProperty]
        private string searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchAuditLogsAsync();
        }

        [ObservableProperty]
        private string statusMessage = "Audit log ekranı hazır.";

        [RelayCommand]
        private async Task LoadAuditLogsAsync()
        {
            AuditLogs.Clear();

            var logs = await _auditLogService.GetAllAsync();

            foreach (var log in logs)
            {
                AuditLogs.Add(log);
            }

            StatusMessage = $"{AuditLogs.Count} audit log kaydı listelendi.";
        }

        [RelayCommand]
        private async Task SearchAuditLogsAsync()
        {
            AuditLogs.Clear();

            var logs = await _auditLogService.SearchAsync(SearchText);

            foreach (var log in logs)
            {
                AuditLogs.Add(log);
            }

            StatusMessage = $"{AuditLogs.Count} sonuç bulundu.";
        }
    }
}