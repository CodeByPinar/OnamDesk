using OnamDesk.Services;
using OnamDesk.Views;
using System.Windows;

namespace OnamDesk
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                var databaseInitializerService = new DatabaseInitializerService();
                await databaseInitializerService.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Veritabanı başlatılırken hata oluştu:\n\n{ex.Message}",
                    "OnamDesk Başlatma Hatası",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }

            var loginView = new LoginView();

            var loginResult = loginView.ShowDialog();

            if (loginResult == true && loginView.IsAuthenticated)
            {
                var mainWindow = new MainWindow();

                MainWindow = mainWindow;

                ShutdownMode = ShutdownMode.OnMainWindowClose;

                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}