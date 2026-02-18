using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace POS.UI.Modules.Suppliers.PurchaseReturn
{
    public class CreatePurchaseReturnViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly PurchaseReturnApiService _service;
        private readonly Guid? _purchaseReturnId;
        private readonly bool _isReadOnly;

        public event Action? OnSaved;
        public Action? CloseAction { get; set; }

        // ================= BUSY INDICATOR =================

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        // ================= HEADER FIELDS =================

        private Guid _supplierId;
        public Guid SupplierId
        {
            get => _supplierId;
            set
            {
                _supplierId = value;
                OnPropertyChanged();
                ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
                // Load purchase entries when supplier changes
                if (value != Guid.Empty && !_isReadOnly)
                {
                    _ = LoadPurchaseEntriesAsync(value);
                }
                else
                {
                    PurchaseEntries.Clear();
                    SelectedPurchaseEntry = null;
                }
            }
        }

        private string? _returnNo;
        public string? ReturnNo
        {
            get => _returnNo;
            set
            {
                _returnNo = value;
                OnPropertyChanged();
                ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged();
            }
        }

        private DateTime _returnDate = DateTime.Now;
        public DateTime ReturnDate
        {
            get => _returnDate;
            set { _returnDate = value; OnPropertyChanged(); }
        }

        private string? _reason;
        public string? Reason
        {
            get => _reason;
            set { _reason = value; OnPropertyChanged(); }
        }

        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private string _status = "Draft";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // ================= SUPPLIERS & PURCHASE ENTRIES =================

        public ObservableCollection<SupplierDto> Suppliers { get; set; } = new();
        public ObservableCollection<PurchaseEntryDto> PurchaseEntries { get; set; } = new();

        private PurchaseEntryDto? _selectedPurchaseEntry;
        public PurchaseEntryDto? SelectedPurchaseEntry
        {
            get => _selectedPurchaseEntry;
            set
            {
                _selectedPurchaseEntry = value;
                OnPropertyChanged();
                ((RelayCommand)LoadFromPurchaseEntryCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= ITEMS COLLECTION =================

        public ObservableCollection<PurchaseReturnItemRowViewModel> Items { get; set; } = new();

        private PurchaseReturnItemRowViewModel? _selectedItem;
        public PurchaseReturnItemRowViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                ((RelayCommand)RemoveItemCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= TOTALS =================

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set { _taxAmount = value; OnPropertyChanged(); }
        }

        // ================= PRODUCT SEARCH =================

        private string _productSearchText = string.Empty;
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value) && value.Length >= 2)
                {
                    _ = SearchProductsAsync(value);
                }
                else
                {
                    ProductSearchResults.Clear();
                    IsProductSearchPopupOpen = false;
                }
            }
        }

        public ObservableCollection<ProductDto> ProductSearchResults { get; set; } = new();

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
                if (_selectedProduct != value && value != null)
                {
                    _selectedProduct = value;
                    OnPropertyChanged();
                    AddProductToGrid(value);
                    ProductSearchText = string.Empty;
                    ProductSearchResults.Clear();
                    IsProductSearchPopupOpen = false;
                }
            }
        }

        // ================= COMMANDS =================

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadSuppliersCommand { get; }
        public ICommand LoadFromPurchaseEntryCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand FocusProductSearchCommand { get; }

        // ================= CONSTRUCTOR =================

        public CreatePurchaseReturnViewModel(PurchaseReturnApiService service, Guid? purchaseReturnId = null, bool isReadOnly = false)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _purchaseReturnId = purchaseReturnId;
            _isReadOnly = isReadOnly;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isReadOnly && CanSave());
            CancelCommand = new RelayCommand(() => CloseWindow());
            LoadSuppliersCommand = new RelayCommand(async () => await LoadSuppliersAsync());
            LoadFromPurchaseEntryCommand = new RelayCommand(() => LoadFromPurchaseEntry(), () => !_isReadOnly && SelectedPurchaseEntry != null);
            AddItemCommand = new RelayCommand(() => { /* Not used - products added via search */ }, () => !_isReadOnly);
            RemoveItemCommand = new RelayCommand(() => RemoveItem(), () => !_isReadOnly && SelectedItem != null);
            FocusProductSearchCommand = new RelayCommand(() => { /* Handled in View */ });

            _ = LoadSuppliersAsync();

            if (_purchaseReturnId.HasValue)
            {
                _ = LoadPurchaseReturnAsync(_purchaseReturnId.Value);
            }
        }

        // ================= METHODS =================

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var supplierService = App.ServiceProvider?.GetService(typeof(SupplierApiService)) as SupplierApiService;
                if (supplierService != null)
                {
                    var suppliers = await supplierService.GetAllAsync(includeInactive: false);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Suppliers.Clear();
                        foreach (var supplier in suppliers)
                        {
                            Suppliers.Add(supplier);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load suppliers.\n{ex.Message}");
            }
        }

        private async Task LoadPurchaseEntriesAsync(Guid supplierId)
        {
            try
            {
                var purchaseEntryService = App.ServiceProvider?.GetService(typeof(PurchaseEntryApiService)) as PurchaseEntryApiService;
                if (purchaseEntryService != null)
                {
                    var entries = await purchaseEntryService.GetBySupplierAsync(supplierId);
                    // Only show processed entries (those with stock to return)
                    var processedEntries = entries?.Where(e => e.IsProcessed).ToList() ?? new List<PurchaseEntryDto>();
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PurchaseEntries.Clear();
                        foreach (var entry in processedEntries)
                        {
                            PurchaseEntries.Add(entry);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load purchase entries.\n{ex.Message}");
            }
        }

        private async Task LoadPurchaseReturnAsync(Guid id)
        {
            try
            {
                IsBusy = true;
                var purchaseReturn = await _service.GetByIdAsync(id);
                if (purchaseReturn != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SupplierId = purchaseReturn.SupplierId;
                        ReturnNo = purchaseReturn.ReturnNo;
                        ReturnDate = purchaseReturn.ReturnDate;
                        Reason = purchaseReturn.Reason;
                        Notes = purchaseReturn.Notes;
                        Status = purchaseReturn.Status;
                        TotalAmount = purchaseReturn.TotalAmount;
                        TaxAmount = purchaseReturn.TaxAmount;

                        Items.Clear();
                        if (purchaseReturn.Items != null)
                        {
                            foreach (var item in purchaseReturn.Items)
                            {
                                var row = new PurchaseReturnItemRowViewModel
                                {
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    ProductCode = item.ProductCode,
                                    PurchaseEntryItemId = item.PurchaseEntryItemId,
                                    BatchNo = item.BatchNo,
                                    ExpiryDate = item.ExpiryDate,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    TaxAmount = item.TaxAmount,
                                    TotalAmount = item.TotalAmount,
                                    Reason = item.Reason
                                };
                                row.PropertyChanged += ItemRow_PropertyChanged;
                                Items.Add(row);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Error", $"Failed to load purchase return.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadFromPurchaseEntry()
        {
            if (SelectedPurchaseEntry == null) return;

            try
            {
                Items.Clear();

                if (SelectedPurchaseEntry.Items != null)
                {
                    foreach (var item in SelectedPurchaseEntry.Items)
                    {
                        var row = new PurchaseReturnItemRowViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ProductCode = item.ProductSKU,
                            PurchaseEntryItemId = item.PurchaseEntryItemId,
                            BatchNo = item.BatchNo,
                            ExpiryDate = item.ExpiryDate,
                            Quantity = 0, // User enters return qty
                            MaxQuantity = item.Quantity, // Original purchase qty for validation
                            UnitPrice = item.CostPrice,
                            TaxAmount = 0,
                            TotalAmount = 0,
                            Reason = string.Empty
                        };
                        row.PropertyChanged += ItemRow_PropertyChanged;
                        Items.Add(row);
                    }
                }

                CalculateTotals();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Error", $"Failed to load items from purchase entry.\n{ex.Message}");
            }
        }

        private async Task SearchProductsAsync(string searchText)
        {
            try
            {
                var productService = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
                if (productService != null)
                {
                    var products = await productService.SearchAsync(searchText);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProductSearchResults.Clear();
                        foreach (var product in products.Take(10))
                        {
                            ProductSearchResults.Add(product);
                        }
                        IsProductSearchPopupOpen = ProductSearchResults.Count > 0;
                    });
                }
            }
            catch (Exception)
            {
                // Silently fail for search
            }
        }

        private void AddProductToGrid(ProductDto product)
        {
            if (_isReadOnly) return;

            // Check if product already exists
            var existing = Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity += 1;
                return;
            }

            var row = new PurchaseReturnItemRowViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                ProductCode = product.SKU,
                Quantity = 1,
                UnitPrice = product.CostPrice,
                TaxAmount = 0,
                TotalAmount = product.CostPrice
            };
            row.PropertyChanged += ItemRow_PropertyChanged;
            Items.Add(row);
            CalculateTotals();
        }

        private void RemoveItem()
        {
            if (SelectedItem == null || _isReadOnly) return;
            
            SelectedItem.PropertyChanged -= ItemRow_PropertyChanged;
            Items.Remove(SelectedItem);
            SelectedItem = null;
            CalculateTotals();
        }

        private void ItemRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PurchaseReturnItemRowViewModel.Quantity) ||
                e.PropertyName == nameof(PurchaseReturnItemRowViewModel.UnitPrice) ||
                e.PropertyName == nameof(PurchaseReturnItemRowViewModel.TaxAmount))
            {
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            TaxAmount = Items.Sum(i => i.TaxAmount);
            TotalAmount = Items.Sum(i => i.TotalAmount);
        }

        private bool CanSave()
        {
            return SupplierId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(ReturnNo) &&
                   Items.Count > 0 &&
                   Items.All(i => i.Quantity > 0);
        }

        private async Task SaveAsync()
        {
            try
            {
                // Validate return quantities against max quantities
                var invalidItems = Items.Where(i => i.MaxQuantity.HasValue && i.Quantity > i.MaxQuantity.Value).ToList();
                if (invalidItems.Any())
                {
                    var invalidProduct = invalidItems.First();
                    MessageBox.Show(
                        $"Return quantity ({invalidProduct.Quantity}) exceeds purchased quantity ({invalidProduct.MaxQuantity}) for product: {invalidProduct.ProductName}",
                        "Validation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                IsBusy = true;

                var dto = new CreatePurchaseReturnDto
                {
                    SupplierId = SupplierId,
                    PurchaseEntryId = SelectedPurchaseEntry?.PurchaseEntryId,
                    ReturnNo = ReturnNo!,
                    ReturnDate = ReturnDate,
                    Reason = Reason,
                    Notes = Notes,
                    Items = Items.Select(i => new CreatePurchaseReturnItemDto
                    {
                        ProductId = i.ProductId,
                        PurchaseEntryItemId = i.PurchaseEntryItemId,
                        BatchNo = i.BatchNo,
                        ExpiryDate = i.ExpiryDate,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TaxAmount = i.TaxAmount,
                        Reason = i.Reason
                    }).ToList()
                };

                if (_purchaseReturnId.HasValue)
                {
                    await _service.UpdateAsync(_purchaseReturnId.Value, dto);
                    POS.UI.Components.DialogService.Success("Success", "Purchase return updated successfully.");
                }
                else
                {
                    await _service.CreateAsync(dto);
                    POS.UI.Components.DialogService.Success("Success", "Purchase return created successfully.");
                }

                OnSaved?.Invoke();
                CloseWindow();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Error", $"Failed to save purchase return.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CloseWindow()
        {
            CloseAction?.Invoke();
        }
    }

    // ================= ITEM ROW VIEW MODEL =================

    public class PurchaseReturnItemRowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public Guid? PurchaseEntryItemId { get; set; }
        public decimal? MaxQuantity { get; set; }

        private string? _batchNo;
        public string? BatchNo
        {
            get => _batchNo;
            set { _batchNo = value; OnPropertyChanged(); }
        }

        private DateTime? _expiryDate;
        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set { _expiryDate = value; OnPropertyChanged(); }
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                CalculateTotal();
            }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged();
                CalculateTotal();
            }
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set
            {
                _taxAmount = value;
                OnPropertyChanged();
                CalculateTotal();
            }
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        private string? _reason;
        public string? Reason
        {
            get => _reason;
            set { _reason = value; OnPropertyChanged(); }
        }

        private void CalculateTotal()
        {
            TotalAmount = (Quantity * UnitPrice) + TaxAmount;
        }
    }
}
