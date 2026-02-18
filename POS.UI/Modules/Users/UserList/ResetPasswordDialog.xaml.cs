using System.Windows;

namespace POS.UI.Modules.Users.UserList
{
    public partial class ResetPasswordDialog : Window
    {
        public string Username { get; }
        public string? NewPassword { get; private set; }

        public ResetPasswordDialog(string username)
        {
            InitializeComponent();
            DataContext = this;
            Username = username;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                Components.DialogService.Warning("Validation", "Please enter a new password");
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                Components.DialogService.Warning("Validation", "Password must be at least 6 characters");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                Components.DialogService.Warning("Validation", "Passwords do not match");
                return;
            }

            NewPassword = NewPasswordBox.Password;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
