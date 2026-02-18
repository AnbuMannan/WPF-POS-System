using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.PurchaseReturn
{
    public partial class CreatePurchaseReturnView : System.Windows.Controls.UserControl
    {
        public CreatePurchaseReturnView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                Keyboard.Focus(this);
            };

            ProductSearchBox.PreviewKeyDown += ProductSearchBox_PreviewKeyDown;
            ProductSearchListBox.PreviewKeyDown += ProductSearchListBox_PreviewKeyDown;
        }

        private void ProductSearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Down && ProductSearchListBox.Items.Count > 0)
            {
                ProductSearchListBox.Focus();
                ProductSearchListBox.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && ProductSearchListBox.Items.Count == 1)
            {
                ProductSearchListBox.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                var vm = DataContext as CreatePurchaseReturnViewModel;
                if (vm != null)
                {
                    vm.IsProductSearchPopupOpen = false;
                }
                e.Handled = true;
            }
        }

        private void ProductSearchListBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ProductSearchListBox.SelectedItem != null)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ProductSearchBox.Focus();
                e.Handled = true;
            }
        }
    }
}
