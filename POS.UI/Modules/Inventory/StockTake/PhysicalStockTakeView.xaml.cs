using System.Windows.Controls;

namespace POS.UI.Modules.Inventory.StockTake
{
    /// <summary>
    /// Interaction logic for PhysicalStockTakeView.xaml
    /// </summary>
    public partial class PhysicalStockTakeView : System.Windows.Controls.UserControl
    {
        public PhysicalStockTakeView()
        {
            InitializeComponent();
            this.Loaded += (s, e) => TxtBarcode.Focus();
        }
    }
}