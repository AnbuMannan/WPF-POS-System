using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Products.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Admin.Products
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly ProductApiService _service;

        public ObservableCollection<CategoryDto> Products { get; set; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        private CategoryDto _selectedProduct;

        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        public CategoryDto SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();

            }

        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();

                // 🔥 Live search with debounce
                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }

        private bool _showInactive;
        public bool ShowInactive
        {
            get => _showInactive;
            set
            {
                _showInactive = value;
                OnPropertyChanged();
                _ = LoadAsync();   // 🔥 Reload when toggled
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }

        public ProductViewModel()
        {
            _service = new ProductApiService(App.ApiClient);
            Products = new ObservableCollection<CategoryDto>();

            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());

            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)   // 400ms debounce
            };
            _searchTimer.Tick += async (s, e) =>
            {
                _searchTimer.Stop();
                await SearchAsync();
            };


            AddCommand = new RelayCommand(OpenAdd);
            EditCommand = new RelayCommand(OpenEdit, () => SelectedProduct != null);
            DisableCommand = new RelayCommand(DisableSelected, () => SelectedProduct != null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            Guid? selectedId = SelectedProduct?.ProductId;
            var list = await _service.GetAllAsync(ShowInactive);

            //System.Windows.MessageBox.Show($"Loaded {list.Count} products");

            Products.Clear();
            foreach (var p in list)
                Products.Add(p);

            // 🔥 Restore selection
            if (selectedId != null)
            {
                SelectedProduct = Products.FirstOrDefault(x => x.ProductId == selectedId);
            }
        }


        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadAsync();
                return;
            }

            var list = await _service.SearchAsync(SearchText);
            Products.Clear();
            foreach (var p in list)
                Products.Add(p);
        }

        private void OpenAdd()
        {
            var window = new ProductFormView();   // only ONE window
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (window.ShowDialog() == true)
            {
                _ = LoadAsync();
            }

        }
        private void OpenEdit()
        {
            if (SelectedProduct == null)
                return;

            var window = new ProductFormView(SelectedProduct);   // pass dto here
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (window.ShowDialog() == true)
            {
                _ = LoadAsync();
            }

        }

        private async void DisableSelected()
        {
            if (SelectedProduct == null)
                return;

            // 🔥 Confirmation dialog
            var result = MessageBox.Show(
                $"Are you sure you want to disable this product?\n\n{SelectedProduct.Name}",
                "Confirm Disable",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _service.DisableAsync(SelectedProduct.ProductId);

                MessageBox.Show("Product disabled successfully",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadAsync();   // 🔥 Refresh grid after disable
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Disable Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
