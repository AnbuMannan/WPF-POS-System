using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.SupplierPayments
{
    public partial class SupplierPaymentView : System.Windows.Controls.UserControl
    {
        public SupplierPaymentView()
        {
            InitializeComponent();
            Loaded += SupplierPaymentView_Loaded;
        }

        private void SupplierPaymentView_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus on search box when view loads
            SearchBox?.Focus();
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle Ctrl+F to focus search
            if (e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SearchBox?.Focus();
                SearchBox?.SelectAll();
                e.Handled = true;
                return;
            }

            // Delegate other keyboard handling to ViewModel
            if (DataContext is SupplierPaymentViewModel vm)
            {
                vm.HandleKeyDown(e);
            }
        }
    }
}
