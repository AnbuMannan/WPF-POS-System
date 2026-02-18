using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Key = System.Windows.Input.Key;
using POS.Shared.Models;
using POS.UI.Core;
using POS.UI.Core.MVVM;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;
using POS.UI.Core.Services;
using Serilog;

namespace POS.UI.Modules.Inventory.LabelPrinting;

/// <summary>
/// ViewModel for Label Printing - supports manual product selection and import from Purchase Entry
/// </summary>
public class LabelPrintingViewModel : INotifyPropertyChanged
{
    private readonly ProductApiService _productApi;
    private readonly PurchaseEntryApiService _purchaseEntryApi;
    private readonly IPrintService _printService;
    private readonly ILogger _logger;

    public LabelPrintingViewModel(
        ProductApiService productApi, 
        PurchaseEntryApiService purchaseEntryApi,
        IPrintService printService)
    {
        _productApi = productApi;
        _purchaseEntryApi = purchaseEntryApi;
        _printService = printService;
        _logger = Log.ForContext<LabelPrintingViewModel>();

        // Initialize collections
        Products = new ObservableCollection<ProductDto>();
        FilteredProducts = new ObservableCollection<ProductDto>();
        PurchaseEntries = new ObservableCollection<PurchaseEntryDto>();
        LabelItems = new ObservableCollection<LabelPrintItem>();

        // Initialize commands
        AddItemCommand = new RelayCommand(() => AddItem(), () => SelectedProduct != null);
        RemoveItemCommand = new RelayCommand(() => RemoveItem(), () => SelectedLabelItem != null);
        ClearAllCommand = new RelayCommand(() => ClearAll(), () => LabelItems.Count > 0);
        ImportFromEntryCommand = new RelayCommand(async () => await ImportFromEntryAsync(), () => SelectedPurchaseEntry != null);
        PrintLabelsCommand = new RelayCommand(async () => await PrintLabelsAsync(), () => LabelItems.Count > 0 && TotalLabelCount > 0);
        RefreshCommand = new RelayCommand(async () => await RefreshAsync());

        // Load initial data
        _ = LoadDataAsync();
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
            IsProductSearchPopupOpen = false;
            if (value != null)
            {
                ProductSearchText = value.Name;
                AddItem();
            }
        }
    }

    private ObservableCollection<PurchaseEntryDto> _purchaseEntries = new();
    public ObservableCollection<PurchaseEntryDto> PurchaseEntries
    {
        get => _purchaseEntries;
        set { _purchaseEntries = value; OnPropertyChanged(); }
    }

    private PurchaseEntryDto? _selectedPurchaseEntry;
    public PurchaseEntryDto? SelectedPurchaseEntry
    {
        get => _selectedPurchaseEntry;
        set { _selectedPurchaseEntry = value; OnPropertyChanged(); }
    }

    private ObservableCollection<LabelPrintItem> _labelItems = new();
    public ObservableCollection<LabelPrintItem> LabelItems
    {
        get => _labelItems;
        set { _labelItems = value; OnPropertyChanged(); }
    }

    private LabelPrintItem? _selectedLabelItem;
    public LabelPrintItem? SelectedLabelItem
    {
        get => _selectedLabelItem;
        set { _selectedLabelItem = value; OnPropertyChanged(); }
    }

    public int TotalLabelCount => LabelItems.Sum(i => i.PrintCount);

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    private bool _isPrinting;
    public bool IsPrinting
    {
        get => _isPrinting;
        set { _isPrinting = value; OnPropertyChanged(); }
    }

    private string _statusMessage = "Add products manually or import from a Purchase Entry";
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    private int _printProgress;
    public int PrintProgress
    {
        get => _printProgress;
        set { _printProgress = value; OnPropertyChanged(); }
    }

    #endregion

    #region Commands

    public ICommand AddItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand ImportFromEntryCommand { get; }
    public ICommand PrintLabelsCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Methods

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading data...";

            // Load products
            var products = await _productApi.GetAllAsync();
            Products.Clear();
            foreach (var product in products.Where(p => p.IsActive))
            {
                Products.Add(product);
            }

            // Load recent purchase entries (last 30 days)
            var fromDate = DateTime.Today.AddDays(-30);
            var entries = await _purchaseEntryApi.GetAllAsync();
            PurchaseEntries.Clear();
            foreach (var entry in entries.OrderByDescending(e => e.ReceivedDate).Take(50))
            {
                PurchaseEntries.Add(entry);
            }

            StatusMessage = $"Loaded {Products.Count} products and {PurchaseEntries.Count} purchase entries";
            _logger.Information("Loaded {ProductCount} products, {EntryCount} purchase entries", 
                Products.Count, PurchaseEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load data for label printing");
            StatusMessage = "Failed to load data. Check API connection.";
        }
        finally
        {
            IsLoading = false;
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

    private void AddItem()
    {
        if (SelectedProduct == null) return;

        // Check if product already exists in list
        var existingItem = LabelItems.FirstOrDefault(i => i.ProductId == SelectedProduct.ProductId);
        if (existingItem != null)
        {
            existingItem.PrintCount++;
            OnPropertyChanged(nameof(TotalLabelCount));
            StatusMessage = $"Increased count for {existingItem.ProductName}";
        }
        else
        {
            var newItem = new LabelPrintItem
            {
                ProductId = SelectedProduct.ProductId,
                ProductName = SelectedProduct.Name,
                SKU = SelectedProduct.SKU,
                Barcode = SelectedProduct.Barcode ?? SelectedProduct.SKU,
                SellingPrice = SelectedProduct.SellingPrice,
                MRP = SelectedProduct.MRP,
                PrintCount = 1
            };
            newItem.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(LabelPrintItem.PrintCount))
                    OnPropertyChanged(nameof(TotalLabelCount));
            };
            LabelItems.Add(newItem);
            StatusMessage = $"Added {SelectedProduct.Name} to print list";
        }

        OnPropertyChanged(nameof(TotalLabelCount));
        
        // Clear selection
        _selectedProduct = null;
        ProductSearchText = string.Empty;
    }

    private void RemoveItem()
    {
        if (SelectedLabelItem == null) return;

        var itemName = SelectedLabelItem.ProductName;
        LabelItems.Remove(SelectedLabelItem);
        SelectedLabelItem = null;
        OnPropertyChanged(nameof(TotalLabelCount));
        StatusMessage = $"Removed {itemName} from print list";
    }

    private void ClearAll()
    {
        LabelItems.Clear();
        SelectedLabelItem = null;
        OnPropertyChanged(nameof(TotalLabelCount));
        StatusMessage = "Cleared all items from print list";
    }

    private async Task ImportFromEntryAsync()
    {
        if (SelectedPurchaseEntry == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Importing items from purchase entry...";

            // Get full entry details with items
            var entry = await _purchaseEntryApi.GetByIdAsync(SelectedPurchaseEntry.PurchaseEntryId);
            if (entry?.Items == null || entry.Items.Count == 0)
            {
                StatusMessage = "No items found in selected purchase entry";
                return;
            }

            int addedCount = 0;
            foreach (var item in entry.Items)
            {
                // Check if product already exists
                var existingItem = LabelItems.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.PrintCount += (int)Math.Ceiling(item.Quantity);
                }
                else
                {
                    var newItem = new LabelPrintItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName ?? $"Product {item.ProductId}",
                        SKU = item.ProductSKU ?? string.Empty,
                        Barcode = item.ProductSKU ?? string.Empty,
                        SellingPrice = item.SellingPrice,
                        MRP = item.MRP,
                        PrintCount = (int)Math.Ceiling(item.Quantity)
                    };
                    newItem.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(LabelPrintItem.PrintCount))
                            OnPropertyChanged(nameof(TotalLabelCount));
                    };
                    LabelItems.Add(newItem);
                    addedCount++;
                }
            }

            OnPropertyChanged(nameof(TotalLabelCount));
            StatusMessage = $"Imported {addedCount} items from {SelectedPurchaseEntry.InvoiceNo}";
            _logger.Information("Imported {Count} items from purchase entry {RefNo}", 
                addedCount, SelectedPurchaseEntry.InvoiceNo);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to import from purchase entry");
            StatusMessage = "Failed to import items";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task PrintLabelsAsync()
    {
        if (LabelItems.Count == 0 || TotalLabelCount == 0)
        {
            StatusMessage = "No labels to print";
            return;
        }

        try
        {
            IsPrinting = true;
            PrintProgress = 0;
            StatusMessage = "Printing labels...";

            int totalLabels = TotalLabelCount;
            int printedCount = 0;

            foreach (var item in LabelItems)
            {
                for (int i = 0; i < item.PrintCount; i++)
                {
                    var success = await _printService.PrintBarcodeLabel(
                        item.Barcode,
                        item.ProductName,
                        item.SellingPrice);

                    if (!success)
                    {
                        _logger.Warning("Failed to print label for {ProductName}", item.ProductName);
                    }

                    printedCount++;
                    PrintProgress = (int)((double)printedCount / totalLabels * 100);
                    
                    // Small delay between prints to prevent printer buffer overflow
                    await Task.Delay(100);
                }
            }

            // Get the labels folder path
            var labelsFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Labels");
            
            StatusMessage = $"Successfully saved {printedCount} labels to Labels folder";
            _logger.Information("Saved {Count} labels to {Folder}", printedCount, labelsFolder);

            // Ask if user wants to clear the list or open folder
            var result = MessageBox.Show(
                $"Labels saved successfully to:\n{labelsFolder}\n\nDo you want to open the Labels folder?",
                "Print Complete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Open the Labels folder in Windows Explorer
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", labelsFolder);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Could not open Labels folder");
                }
            }

            // Ask if user wants to clear the list
            var clearResult = MessageBox.Show(
                "Do you want to clear the labels list?",
                "Clear List",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (clearResult == MessageBoxResult.Yes)
            {
                ClearAll();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error printing labels");
            StatusMessage = "Error occurred while printing labels";
            MessageBox.Show(
                $"Error printing labels: {ex.Message}",
                "Print Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsPrinting = false;
            PrintProgress = 0;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            _ = RefreshAsync();
            e.Handled = true;
        }
        else if (e.Key == Key.F2)
        {
            if (LabelItems.Count > 0 && TotalLabelCount > 0)
            {
                _ = PrintLabelsAsync();
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            if (IsProductSearchPopupOpen)
            {
                IsProductSearchPopupOpen = false;
            }
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && SelectedLabelItem != null)
        {
            RemoveItem();
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

/// <summary>
/// Represents an item in the label print queue
/// </summary>
public class LabelPrintItem : INotifyPropertyChanged
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal MRP { get; set; }

    private int _printCount = 1;
    public int PrintCount
    {
        get => _printCount;
        set
        {
            if (value >= 0)
            {
                _printCount = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
