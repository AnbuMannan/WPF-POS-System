using System.Windows;

namespace POS.UI.Modules.Users.Roles
{
    public partial class AddRoleDialog : Window
    {
        public string? RoleName { get; private set; }
        public string? RoleDescription { get; private set; }

        public AddRoleDialog()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoleNameBox.Text))
            {
                Components.DialogService.Warning("Validation", "Role name is required");
                return;
            }

            RoleName = RoleNameBox.Text.Trim();
            RoleDescription = DescriptionBox.Text.Trim();
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
