using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using POS.Shared.Models;

namespace POS.UI.Modules.Billing.ProductSearch
{
    public partial class ProductSearchControl : System.Windows.Controls.UserControl
    {
        private DispatcherTimer? _searchTimer;
        public event Action<ProductDto>? ProductSelected;

        public ProductSearchControl()
        {
            InitializeComponent();
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += SearchTimer_Tick;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer?.Stop();
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchPopup.IsOpen = false;
                return;
            }
            _searchTimer?.Start();
        }

        private async void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer?.Stop();
            if (DataContext is not ProductSearchViewModel vm) return;

            var searchText = vm.SearchText?.Trim();
            if (string.IsNullOrWhiteSpace(searchText)) return;

            // Trigger search in parent ViewModel (BillingViewModel will handle this)
            await PerformSearch(searchText);
        }

        private async Task PerformSearch(string searchText)
        {
            if (DataContext is not ProductSearchViewModel vm) return;

            try
            {
                vm.IsSearching = true;
                
                // Wait a bit for the parent BillingViewModel to perform the search
                await Task.Delay(100);
                
                if (vm.SearchResults.Count > 0)
                {
                    SearchPopup.IsOpen = true;
                    SearchResultsList.SelectedIndex = 0;
                }
                else
                {
                    SearchPopup.IsOpen = false;
                }
            }
            finally
            {
                vm.IsSearching = false;
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Down && SearchPopup.IsOpen && SearchResultsList.Items.Count > 0)
            {
                SearchResultsList.Focus();
                SearchResultsList.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && SearchPopup.IsOpen)
            {
                AddSelectedProduct();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                SearchPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void SearchResultsList_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddSelectedProduct();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                SearchPopup.IsOpen = false;
                SearchBox.Focus();
                e.Handled = true;
            }
        }

        private void SearchResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsList.SelectedItem is ProductDto product)
            {
                if (DataContext is ProductSearchViewModel vm)
                {
                    vm.SelectedProduct = product;
                }
            }
        }

        private void SearchResultsList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox == null) return;

            // Find the clicked item
            var item = ItemsControl.ContainerFromElement(listBox, e.OriginalSource as DependencyObject) as ListBoxItem;

            if (item != null && item.DataContext is ProductDto product)
            {
                listBox.SelectedItem = product;
                AddSelectedProduct();
                e.Handled = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddSelectedProduct();
        }

        private void AddSelectedProduct()
        {
            if (DataContext is ProductSearchViewModel vm && vm.SelectedProduct != null)
            {
                ProductSelected?.Invoke(vm.SelectedProduct);
                
                // Clear search
                vm.SearchText = string.Empty;
                vm.SearchResults.Clear();
                vm.SelectedProduct = null;
                SearchPopup.IsOpen = false;
                SearchBox.Focus();
            }
        }

        public void FocusSearchBox()
        {
            SearchBox?.Focus();
        }
    }
}
