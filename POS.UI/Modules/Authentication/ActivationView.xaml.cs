using System.Windows.Controls;

using System.Windows;
using POS.UI.Modules.Utilities.SystemHealth;

namespace POS.UI.Modules.Authentication
{
    public partial class ActivationView : System.Windows.Controls.UserControl
    {
        public ActivationView()
        {
            InitializeComponent();
        }

        private void OpenSystemHealth_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SystemHealthDialog { Owner = System.Windows.Application.Current.MainWindow };
            dlg.ShowDialog();
        }
    }
}
