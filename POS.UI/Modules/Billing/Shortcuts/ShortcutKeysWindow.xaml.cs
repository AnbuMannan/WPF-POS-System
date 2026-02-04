using System.Windows;

namespace POS.UI.Modules.Billing.Shortcuts
{
    public partial class ShortcutKeysWindow : Window
    {
        public ShortcutKeysWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
