using System.Windows;

namespace POS.UI.Modules.Utilities.SystemHealth
{
    public partial class SystemHealthDialog : Window
    {
        public SystemHealthDialog()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(SystemHealthViewModel)) as SystemHealthViewModel;
            Owner = System.Windows.Application.Current?.MainWindow;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
