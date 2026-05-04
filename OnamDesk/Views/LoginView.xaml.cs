using OnamDesk.ViewModels;
using System.Windows;

namespace OnamDesk.Views
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;

        public bool IsAuthenticated { get; private set; }

        public LoginView()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();

            _viewModel.LoginSucceeded += OnLoginSucceeded;

            DataContext = _viewModel;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordInput.Password;
        }

        private void OnLoginSucceeded()
        {
            IsAuthenticated = true;

            DialogResult = true;

            Close();
        }
    }
}
