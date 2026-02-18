using System.Windows;
using System.Windows.Controls;

namespace POS.UI.Modules.Suppliers.SupplierLedger
{
    public partial class SupplierLedgerView : System.Windows.Controls.UserControl
    {
        public SupplierLedgerView()
        {
            InitializeComponent();
            Loaded += SupplierLedgerView_Loaded;
        }

        private void SupplierLedgerView_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on the control when loaded
            Focus();
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Delegate keyboard handling to ViewModel
            if (DataContext is SupplierLedgerViewModel vm)
            {
                vm.HandleKeyDown(e);
            }
        }
    }
}
