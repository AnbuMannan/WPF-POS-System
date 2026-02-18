using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.PurchaseOrder
{
    public partial class CreatePurchaseOrderView : System.Windows.Controls.UserControl
    {
        public CreatePurchaseOrderView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Focus first control (Supplier)
                Keyboard.Focus(this);
            };
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.F1)
            {
                e.Handled = true;
                ProductSearchBox?.Focus();
                ProductSearchBox?.SelectAll();
            }
        }

        private void ProductSearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not CreatePurchaseOrderViewModel vm) return;

            // Handle DOWN arrow to focus ListBox
            if (e.Key == Key.Down && vm.ProductSearchResults.Count > 0)
            {
                e.Handled = true;
                ProductSearchListBox?.Focus();
                if (ProductSearchListBox?.Items.Count > 0)
                {
                    ProductSearchListBox.SelectedIndex = 0;
                }
            }

            // Handle ENTER when only one result
            if (e.Key == Key.Enter && vm.ProductSearchResults.Count == 1)
            {
                e.Handled = true;
                vm.SelectedProduct = vm.ProductSearchResults[0];
            }

            // Handle ESC to close popup
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                vm.IsProductSearchPopupOpen = false;
                vm.ProductSearchText = string.Empty;
            }
        }

        private void ProductSearchListBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not CreatePurchaseOrderViewModel vm) return;

            // Handle ENTER to select
            if (e.Key == Key.Enter && ProductSearchListBox?.SelectedItem is Shared.Models.ProductDto product)
            {
                e.Handled = true;
                vm.SelectedProduct = product;
            }

            // Handle ESC to go back to search box
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                ProductSearchBox?.Focus();
            }
        }
    }
}
