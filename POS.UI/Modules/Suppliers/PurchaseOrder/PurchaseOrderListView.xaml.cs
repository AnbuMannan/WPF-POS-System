using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.PurchaseOrder
{
    public partial class PurchaseOrderListView : System.Windows.Controls.UserControl
    {
        public PurchaseOrderListView()
        {
            InitializeComponent();
            // Set focus to enable keyboard shortcuts
            Loaded += (s, e) => { Keyboard.Focus(this); };
        }
    }
}
