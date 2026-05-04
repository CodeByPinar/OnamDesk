using OnamDesk.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OnamDesk.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsView()
        {
            InitializeComponent();

            _viewModel = new SettingsViewModel();

            DataContext = _viewModel;
        }

        private void CurrentPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentPassword = CurrentPasswordInput.Password;
        }

        private void NewPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.NewPassword = NewPasswordInput.Password;
        }

        private void ConfirmNewPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.ConfirmNewPassword = ConfirmNewPasswordInput.Password;
        }
    }
}