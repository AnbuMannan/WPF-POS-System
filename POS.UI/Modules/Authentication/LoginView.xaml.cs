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
            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
            };


            // Allow ViewModel to clear password securely
            viewModel.ClearPasswordAction = () => PasswordBox.Password = string.Empty;
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
    }
}
