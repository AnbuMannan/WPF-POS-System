using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.PurchaseEntry
{
    public partial class PurchaseEntryListView : System.Windows.Controls.UserControl
    {
        public PurchaseEntryListView()
        {
            InitializeComponent();
            // Set focus to enable keyboard shortcuts
            Loaded += (s, e) => { Keyboard.Focus(this); };
        }
    }
}
