using OnamDesk.Services;
using OnamDesk.ViewModels;
using OnamDesk.Views;
using System.ComponentModel;
using System.Windows;

namespace OnamDesk
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly AuditLogService _auditLogService;
        private readonly SettingsService _settingsService;
        private readonly BackupService _backupService;

        private bool _isBackupRunning;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _auditLogService = new AuditLogService();
            _settingsService = new SettingsService();
            _backupService = new BackupService();

            _viewModel.LogoutRequested += OnLogoutRequested;

            DataContext = _viewModel;
        }

        private async void OnLogoutRequested()
        {
            var auditUserName = await GetAuditUserNameAsync();

            await _auditLogService.AddAsync(
                action: "LOGOUT_REQUESTED",
                userName: auditUserName,
                details: new
                {
                    RequestedAt = DateTime.UtcNow
                });

            var result = MessageBox.Show(
                "Oturumu kapatmak istediğinize emin misiniz?",
                "Çıkış Yap",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                await _auditLogService.AddAsync(
                    action: "LOGOUT_CANCELLED",
                    userName: auditUserName,
                    details: new
                    {
                        CancelledAt = DateTime.UtcNow
                    });

                return;
            }

            await _auditLogService.AddAsync(
                action: "LOGOUT_SUCCESS",
                userName: auditUserName,
                details: new
                {
                    LoggedOutAt = DateTime.UtcNow
                });

            var loginView = new LoginView();

            Hide();

            var loginResult = loginView.ShowDialog();

            if (loginResult == true && loginView.IsAuthenticated)
            {
                var reloginUserName = await GetAuditUserNameAsync();

                await _auditLogService.AddAsync(
                    action: "RELOGIN_SUCCESS",
                    userName: reloginUserName,
                    details: new
                    {
                        LoggedInAt = DateTime.UtcNow
                    });

                Show();
            }
            else
            {
                await _auditLogService.AddAsync(
                    action: "RELOGIN_FAILED_OR_CANCELLED",
                    userName: auditUserName,
                    details: new
                    {
                        ClosedAt = DateTime.UtcNow
                    });

                Close();
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (_isBackupRunning)
            {
                base.OnClosing(e);
                return;
            }

            _isBackupRunning = true;

            try
            {
                var backupPath = _backupService.CreateDatabaseBackup();

                _backupService.DeleteOldBackups(keepLastCount: 10);

                var auditUserName = await GetAuditUserNameAsync();

                await _auditLogService.AddAsync(
                    action: "DATABASE_BACKUP_CREATED",
                    userName: auditUserName,
                    details: new
                    {
                        BackupPath = backupPath,
                        CreatedAt = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                try
                {
                    var auditUserName = await GetAuditUserNameAsync();

                    await _auditLogService.AddAsync(
                        action: "DATABASE_BACKUP_FAILED",
                        userName: auditUserName,
                        details: new
                        {
                            ErrorMessage = ex.Message,
                            FailedAt = DateTime.UtcNow
                        });
                }
                catch
                {
                    // Audit log da yazılamazsa kapanışı engelleme.
                }
            }

            base.OnClosing(e);
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