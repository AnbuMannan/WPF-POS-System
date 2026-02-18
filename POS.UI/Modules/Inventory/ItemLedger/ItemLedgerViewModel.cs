using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using Key = System.Windows.Input.Key;
using Serilog;

namespace POS.UI.Modules.Inventory.ItemLedger;

/// <summary>
/// ViewModel for Item Ledger view - displays transaction history for a product
/// </summary>
public class ItemLedgerViewModel : INotifyPropertyChanged
{
    private readonly ItemLedgerApiService _ledgerApi;
    private readonly ProductApiService _productApi;
    private readonly ILogger _logger;

    public ItemLedgerViewModel(ItemLedgerApiService ledgerApi, ProductApiService productApi)
    {
        _ledgerApi = ledgerApi;
        _productApi = productApi;
        _logger = Log.ForContext<ItemLedgerViewModel>();

        // Initialize collections
        Products = new ObservableCollection<ProductDto>();
        FilteredProducts = new ObservableCollection<ProductDto>();
        LedgerEntries = new ObservableCollection<ItemLedgerDto>();

        // Initialize dates
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;

        // Initialize commands
        SearchCommand = new RelayCommand(async () => await SearchAsync(), () => SelectedProduct != null);
        ClearCommand = new RelayCommand(() => Clear());
        ExportCommand = new RelayCommand(() => Export(), () => LedgerEntries.Count > 0);
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());

        // Load products on initialization
        _ = LoadProductsAsync();
    }

    #region Properties

    private ObservableCollection<ProductDto> _products = new();
    public ObservableCollection<ProductDto> Products
    {
        get => _products;
        set { _products = value; OnPropertyChanged(); }
    }

    private ObservableCollection<ProductDto> _filteredProducts = new();
    public ObservableCollection<ProductDto> FilteredProducts
    {
        get => _filteredProducts;
        set { _filteredProducts = value; OnPropertyChanged(); }
    }

    private string _productSearchText = string.Empty;
    public string ProductSearchText
    {
        get => _productSearchText;
        set
        {
            _productSearchText = value;
            OnPropertyChanged();
            FilterProducts();
            IsProductSearchPopupOpen = !string.IsNullOrEmpty(value) && FilteredProducts.Count > 0;
        }
    }

    private bool _isProductSearchPopupOpen;
    public bool IsProductSearchPopupOpen
    {
        get => _isProductSearchPopupOpen;
        set { _isProductSearchPopupOpen = value; OnPropertyChanged(); }
    }

    private ProductDto? _selectedProduct;
    public ProductDto? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            _selectedProduct = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsProductSelected));
            IsProductSearchPopupOpen = false;
            if (value != null)
            {
                ProductSearchText = value.Name;
                _ = SearchAsync();
            }
        }
    }

    public bool IsProductSelected => SelectedProduct != null;

    private DateTime _fromDate;
    public DateTime FromDate
    {
        get => _fromDate;
        set { _fromDate = value; OnPropertyChanged(); }
    }

    private DateTime _toDate;
    public DateTime ToDate
    {
        get => _toDate;
        set { _toDate = value; OnPropertyChanged(); }
    }

    private ObservableCollection<ItemLedgerDto> _ledgerEntries = new();
    public ObservableCollection<ItemLedgerDto> LedgerEntries
    {
        get => _ledgerEntries;
        set { _ledgerEntries = value; OnPropertyChanged(); }
    }

    private decimal _openingBalance;
    public decimal OpeningBalance
    {
        get => _openingBalance;
        set { _openingBalance = value; OnPropertyChanged(); }
    }

    private decimal _totalIn;
    public decimal TotalIn
    {
        get => _totalIn;
        set { _totalIn = value; OnPropertyChanged(); }
    }

    private decimal _totalOut;
    public decimal TotalOut
    {
        get => _totalOut;
        set { _totalOut = value; OnPropertyChanged(); }
    }

    private decimal _closingBalance;
    public decimal ClosingBalance
    {
        get => _closingBalance;
        set { _closingBalance = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    private string _statusMessage = "Select a product to view its transaction history";
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    #endregion

    #region Commands

    public ICommand SearchCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            var products = await _productApi.GetAllAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            _logger.Information("Loaded {Count} products for ledger search", products.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load products");
            StatusMessage = "Failed to load products";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task PreselectProductAsync(long productId)
    {
        if (Products.Count == 0)
        {
            await LoadProductsAsync();
        }

        var product = Products.FirstOrDefault(p => p.ProductId == productId);
        if (product != null)
        {
            SelectedProduct = product;
        }
    }

    private void FilterProducts()
    {
        FilteredProducts.Clear();
        if (string.IsNullOrWhiteSpace(ProductSearchText))
            return;

        var searchTerm = ProductSearchText.ToLower();
        var filtered = Products
            .Where(p => p.Name.ToLower().Contains(searchTerm) ||
                       p.SKU.ToLower().Contains(searchTerm) ||
                       (p.Barcode?.ToLower().Contains(searchTerm) ?? false))
            .Take(20);

        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }
    }

    private async Task SearchAsync()
    {
        if (SelectedProduct == null)
        {
            StatusMessage = "Please select a product first";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Loading ledger...";

            var response = await _ledgerApi.GetLedgerAsync(
                SelectedProduct.ProductId,
                FromDate,
                ToDate);

            if (response == null)
            {
                StatusMessage = "No data found";
                return;
            }

            LedgerEntries.Clear();
            foreach (var entry in response.Entries)
            {
                LedgerEntries.Add(entry);
            }

            OpeningBalance = response.OpeningBalance;
            TotalIn = response.TotalIn;
            TotalOut = response.TotalOut;
            ClosingBalance = response.ClosingBalance;

            StatusMessage = $"Found {LedgerEntries.Count} transactions for {SelectedProduct.Name}";
            _logger.Information("Loaded {Count} ledger entries for product {ProductId}", 
                LedgerEntries.Count, SelectedProduct.ProductId);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load ledger for product {ProductId}", SelectedProduct?.ProductId);
            StatusMessage = "Failed to load ledger. Check API connection.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadProductsAsync();
        if (SelectedProduct != null)
        {
            await SearchAsync();
        }
    }

    private void Clear()
    {
        SelectedProduct = null;
        ProductSearchText = string.Empty;
        LedgerEntries.Clear();
        OpeningBalance = 0;
        TotalIn = 0;
        TotalOut = 0;
        ClosingBalance = 0;
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
        StatusMessage = "Select a product to view its transaction history";
    }

    private void Export()
    {
        // TODO: Implement CSV/Excel export
        StatusMessage = "Export feature coming soon...";
    }

    public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.F5)
        {
            _ = RefreshAsync();
            e.Handled = true;
        }
        else if (e.Key == System.Windows.Input.Key.Escape)
        {
            if (IsProductSearchPopupOpen)
            {
                IsProductSearchPopupOpen = false;
            }
            else
            {
                Clear();
            }
            e.Handled = true;
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
