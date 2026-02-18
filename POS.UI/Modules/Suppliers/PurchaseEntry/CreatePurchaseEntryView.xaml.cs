using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.PurchaseEntry
{
    public partial class CreatePurchaseEntryView : System.Windows.Controls.UserControl
    {
        public CreatePurchaseEntryView()
        {
            InitializeComponent();
            Loaded += (s, e) => { Keyboard.Focus(this); };
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            
            // Handle F1 to focus product search
            if (e.Key == Key.F1)
            {
                e.Handled = true;
                ProductSearchBox?.Focus();
                ProductSearchBox?.SelectAll();
            }
        }
    }
}
