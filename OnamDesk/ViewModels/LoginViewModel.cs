using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnamDesk.Services;

namespace OnamDesk.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly AuditLogService _auditLogService;
        private readonly SettingsService _settingsService;

        public LoginViewModel()
        {
            _authService = new AuthService();
            _auditLogService = new AuditLogService();
            _settingsService = new SettingsService();

            _ = InitializeAsync();
        }

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string statusMessage = "Lütfen uygulama şifresini girin.";

        [ObservableProperty]
        private bool isLoginSuccessful;

        [ObservableProperty]
        private bool isBusy;

        public event Action? LoginSucceeded;

        private async Task InitializeAsync()
        {
            await _authService.EnsureDefaultPasswordAsync();
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Şifre alanı boş bırakılamaz.";
                return;
            }

            try
            {
                IsBusy = true;

                var result = await _authService.LoginAsync(Password);

                StatusMessage = result.Message;

                if (!result.IsSuccess)
                {
                    await WriteLoginAuditAsync("LOGIN_FAILED", result.Message);
                    return;
                }

                IsLoginSuccessful = true;

                await WriteLoginAuditAsync("LOGIN_SUCCESS", "Giriş başarılı.");

                LoginSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Giriş sırasında hata oluştu: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task WriteLoginAuditAsync(string action, string message)
        {
            var settings = await _settingsService.GetSettingsAsync();

            var userName = string.IsNullOrWhiteSpace(settings.CurrentUserName)
                ? "Admin"
                : settings.CurrentUserName.Trim();

            await _auditLogService.AddAsync(
                action: action,
                userName: userName,
                details: new
                {
                    Message = message,
                    AttemptedAt = DateTime.UtcNow
                });
        }
    }
}