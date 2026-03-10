using System;
using System.Windows;
using Application = System.Windows.Application;
using POS.UI.Modules.Utilities.SystemHealth;

namespace POS.UI.Modules.Authentication
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }


        // Loaded event handler (correct WPF pattern)
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                AttachPasswordBox(viewModel);
            }
        }


        private void AttachPasswordBox(LoginViewModel viewModel)
        {
            txtPassword.PasswordChanged += (s, e) =>
            {
                viewModel.Password = txtPassword.Password;
            };


            // Allow ViewModel to clear password securely
            viewModel.ClearPasswordAction = () => txtPassword.Password = string.Empty;
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }

        private void OpenSystemHealth_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SystemHealthDialog { Owner = this };
            dlg.ShowDialog();
        }

        private void TxtUsername_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtPassword.Focus();
                e.Handled = true; // Prevents the IsDefault Login button from firing early
            }
        }
    }
}
