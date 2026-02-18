using System.Windows;
using System.Windows.Controls;

namespace POS.UI.Modules.Inventory.StockAdjustment
{
    public partial class StockAdjustmentListView : System.Windows.Controls.UserControl
    {
        public StockAdjustmentListView()
        {
            InitializeComponent();
            Loaded += StockAdjustmentListView_Loaded;
        }

        private void StockAdjustmentListView_Loaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox?.Focus();
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle Ctrl+F to focus search
            if (e.Key == System.Windows.Input.Key.F && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                SearchTextBox?.Focus();
                SearchTextBox?.SelectAll();
                e.Handled = true;
                return;
            }

            // Delegate other keyboard handling to ViewModel
            if (DataContext is StockAdjustmentListViewModel vm)
            {
                vm.HandleKeyDown(e);
            }
        }
    }
}
