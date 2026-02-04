using System.Windows;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Settings
{
    public partial class PrintSettingsDialog : Window
    {
        public PrintSettingsDialog()
        {
            InitializeComponent();
        }

        public PrintSettingsDialog(IPrintSettingsService printSettings, IPrintService printService)
            : this()
        {
            // Store or use services as needed for print settings
        }
    }
}
