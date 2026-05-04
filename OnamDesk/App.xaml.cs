using OnamDesk.Views;
using System.Windows;

namespace OnamDesk
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Login kapanınca uygulama otomatik kapanmasın.
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginView = new LoginView();

            var loginResult = loginView.ShowDialog();

            if (loginResult == true && loginView.IsAuthenticated)
            {
                var mainWindow = new MainWindow();

                MainWindow = mainWindow;

                // Ana pencere açıldıktan sonra normal kapanma davranışına dön.
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