using System.Windows;
using System.Windows.Controls;

namespace POS.UI.Modules.Inventory.StockAdjustment
{
    public partial class CreateStockAdjustmentView : System.Windows.Controls.UserControl
    {
        public CreateStockAdjustmentView()
        {
            InitializeComponent();
            Loaded += CreateStockAdjustmentView_Loaded;
        }

        private void CreateStockAdjustmentView_Loaded(object sender, RoutedEventArgs e)
        {
            ProductSearchBox?.Focus();
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is CreateStockAdjustmentViewModel vm)
            {
                vm.HandleKeyDown(e);
            }
        }
    }
}
