using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.PurchaseReturn
{
    public partial class PurchaseReturnListView : System.Windows.Controls.UserControl
    {
        public PurchaseReturnListView()
        {
            InitializeComponent();
            Loaded += (s, e) => Keyboard.Focus(this);
        }
    }
}
