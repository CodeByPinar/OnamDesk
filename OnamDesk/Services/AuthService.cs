using Newtonsoft.Json;
using System.IO;

namespace OnamDesk.Services
{
    public class AuthService
    {
        private readonly string _authFolder;
        private readonly string _authFilePath;

        public AuthService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            _authFolder = Path.Combine(appDataPath, "OnamDesk");
            _authFilePath = Path.Combine(_authFolder, "auth.json");

            if (!Directory.Exists(_authFolder))
            {
                Directory.CreateDirectory(_authFolder);
            }
        }

        public async Task EnsureDefaultPasswordAsync()
        {
            if (File.Exists(_authFilePath))
            {
                return;
            }

            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("onam1234");

            var authSettings = new AuthSettings
            {
                PasswordHash = defaultPasswordHash,
                FailedLoginCount = 0,
                LockoutEndUtc = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await SaveAuthSettingsAsync(authSettings);
        }

        public async Task<AuthResult> LoginAsync(string password)
        {
            await EnsureDefaultPasswordAsync();

            var authSettings = await GetAuthSettingsAsync();

            if (authSettings.LockoutEndUtc.HasValue &&
                authSettings.LockoutEndUtc.Value > DateTime.UtcNow)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Message = $"Çok fazla hatalı giriş yapıldı. Kilit bitiş zamanı: {authSettings.LockoutEndUtc.Value.ToLocalTime():dd.MM.yyyy HH:mm:ss}"
                };
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, authSettings.PasswordHash);

            if (!isPasswordValid)
            {
                authSettings.FailedLoginCount++;

                if (authSettings.FailedLoginCount >= 5)
                {
                    authSettings.LockoutEndUtc = DateTime.UtcNow.AddMinutes(2);
                    authSettings.FailedLoginCount = 0;
                }

                authSettings.UpdatedAt = DateTime.UtcNow;

                await SaveAuthSettingsAsync(authSettings);

                return new AuthResult
                {
                    IsSuccess = false,
                    Message = "Şifre hatalı."
                };
            }

            authSettings.FailedLoginCount = 0;
            authSettings.LockoutEndUtc = null;
            authSettings.UpdatedAt = DateTime.UtcNow;

            await SaveAuthSettingsAsync(authSettings);

            return new AuthResult
            {
                IsSuccess = true,
                Message = "Giriş başarılı."
            };
        }

        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            await EnsureDefaultPasswordAsync();

            var authSettings = await GetAuthSettingsAsync();

            var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, authSettings.PasswordHash);

            if (!isCurrentPasswordValid)
            {
                throw new InvalidOperationException("Mevcut şifre hatalı.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                throw new InvalidOperationException("Yeni şifre en az 6 karakter olmalıdır.");
            }

            authSettings.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            authSettings.FailedLoginCount = 0;
            authSettings.LockoutEndUtc = null;
            authSettings.UpdatedAt = DateTime.UtcNow;

            await SaveAuthSettingsAsync(authSettings);
        }

        private async Task<AuthSettings> GetAuthSettingsAsync()
        {
            await EnsureDefaultPasswordAsync();

            var json = await File.ReadAllTextAsync(_authFilePath);

            var authSettings = JsonConvert.DeserializeObject<AuthSettings>(json);

            return authSettings ?? new AuthSettings
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("onam1234"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private async Task SaveAuthSettingsAsync(AuthSettings authSettings)
        {
            if (!Directory.Exists(_authFolder))
            {
                Directory.CreateDirectory(_authFolder);
            }

            var json = JsonConvert.SerializeObject(authSettings, Formatting.Indented);

            await File.WriteAllTextAsync(_authFilePath, json);
        }
    }

    public class AuthSettings
    {
        public string PasswordHash { get; set; } = string.Empty;

        public int FailedLoginCount { get; set; }

        public DateTime? LockoutEndUtc { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}