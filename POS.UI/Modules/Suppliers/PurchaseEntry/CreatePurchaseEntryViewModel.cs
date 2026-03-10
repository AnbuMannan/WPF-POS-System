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

namespace POS.UI.Modules.Suppliers.PurchaseEntry
{
    public class CreatePurchaseEntryViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly PurchaseEntryApiService _service;
        private readonly Guid? _purchaseEntryId;
        private readonly bool _isReadOnly;

        public event Action OnSaved;

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
                // Load pending POs when supplier changes
                if (value != Guid.Empty)
                {
                    _ = LoadPendingPurchaseOrdersAsync(value);
                }
                else
                {
                    PendingPurchaseOrders.Clear();
                    SelectedPurchaseOrder = null;
                }
            }
        }

        private string _supplierName;
        public string SupplierName
        {
            get => _supplierName;
            set { _supplierName = value; OnPropertyChanged(); }
        }

        private Guid? _purchaseOrderId;
        public Guid? PurchaseOrderId
        {
            get => _purchaseOrderId;
            set { _purchaseOrderId = value; OnPropertyChanged(); }
        }

        private string _invoiceNo;
        public string InvoiceNo
        {
            get => _invoiceNo;
            set { _invoiceNo = value; OnPropertyChanged(); ((RelayCommand)SaveCommand)?.RaiseCanExecuteChanged(); }
        }

        private DateTime _invoiceDate = DateTime.Now;
        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set { _invoiceDate = value; OnPropertyChanged(); }
        }

        private DateTime _receivedDate = DateTime.Now;
        public DateTime ReceivedDate
        {
            get => _receivedDate;
            set { _receivedDate = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private bool _isProcessed;
        public bool IsProcessed
        {
            get => _isProcessed;
            set { _isProcessed = value; OnPropertyChanged(); }
        }

        // ================= SUPPLIERS & PO LIST =================

        public ObservableCollection<SupplierDto> Suppliers { get; set; } = new();
        public ObservableCollection<PurchaseOrderDto> PendingPurchaseOrders { get; set; } = new();

        private PurchaseOrderDto _selectedPurchaseOrder;
        public PurchaseOrderDto SelectedPurchaseOrder
        {
            get => _selectedPurchaseOrder;
            set
            {
                _selectedPurchaseOrder = value;
                OnPropertyChanged();
                ((RelayCommand)ImportFromPOCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= ITEMS COLLECTION =================

        public ObservableCollection<PurchaseEntryItemRowViewModel> Items { get; set; } = new();

        private PurchaseEntryItemRowViewModel _selectedItem;
        public PurchaseEntryItemRowViewModel SelectedItem
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

        private string _productSearchText;
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
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
            set
            {
                _isProductSearchPopupOpen = value;
                OnPropertyChanged();
            }
        }

        private ProductDto _selectedProduct;
        public ProductDto SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (value != null && !_isReadOnly)
                {
                    AddProductToItems(value);
                    ProductSearchText = string.Empty;
                    ProductSearchResults.Clear();
                    IsProductSearchPopupOpen = false;
                }
            }
        }

        // ================= COMMANDS =================

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand LoadSuppliersCommand { get; }
        public ICommand ImportFromPOCommand { get; }
        public ICommand FocusProductSearchCommand { get; }

        // ================= CONSTRUCTOR =================

        public CreatePurchaseEntryViewModel(PurchaseEntryApiService service, Guid? purchaseEntryId = null, bool isReadOnly = false)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _purchaseEntryId = purchaseEntryId;
            _isReadOnly = isReadOnly;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isReadOnly && CanSave());
            CancelCommand = new RelayCommand(() => CloseWindow());
            AddItemCommand = new RelayCommand(() => ShowProductSearch(), () => !_isReadOnly);
            RemoveItemCommand = new RelayCommand(() => RemoveItem(), () => !_isReadOnly && SelectedItem != null);
            LoadSuppliersCommand = new RelayCommand(async () => await LoadSuppliersAsync());
            ImportFromPOCommand = new RelayCommand(async () => await ImportFromPOAsync(), 
                () => !_isReadOnly && SelectedPurchaseOrder != null);
            FocusProductSearchCommand = new RelayCommand(() => { /* Will be handled in view */ });

            _ = LoadSuppliersAsync();

            if (_purchaseEntryId.HasValue)
            {
                _ = LoadPurchaseEntryAsync(_purchaseEntryId.Value);
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

        private async Task LoadPendingPurchaseOrdersAsync(Guid supplierId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Loading pending POs for supplier: {supplierId}");
                
                var poService = App.ServiceProvider?.GetService(typeof(PurchaseOrderApiService)) as PurchaseOrderApiService;
                if (poService == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] PurchaseOrderApiService is null!");
                    POS.UI.Components.DialogService.Warning("Error", "Purchase Order service not available.");
                    return;
                }
                
                var pendingPOs = await poService.GetPendingBySupplierAsync(supplierId);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {pendingPOs?.Count ?? 0} pending POs");
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PendingPurchaseOrders.Clear();
                    if (pendingPOs != null && pendingPOs.Count > 0)
                    {
                        foreach (var po in pendingPOs)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Adding PO: {po.ReferenceNo} - {po.TotalAmount:C}");
                            PendingPurchaseOrders.Add(po);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] No pending POs found for this supplier");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to load pending POs: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load pending purchase orders.\n{ex.Message}");
            }
        }

        private async Task LoadPurchaseEntryAsync(Guid id)
        {
            try
            {
                IsBusy = true;
                var entry = await _service.GetByIdAsync(id);
                if (entry != null)
                {
                    SupplierId = entry.SupplierId;
                    SupplierName = entry.SupplierName;
                    PurchaseOrderId = entry.PurchaseOrderId;
                    InvoiceNo = entry.InvoiceNo;
                    InvoiceDate = entry.InvoiceDate;
                    ReceivedDate = entry.ReceivedDate;
                    Notes = entry.Notes;
                    IsProcessed = entry.IsProcessed;

                    Items.Clear();
                    foreach (var item in entry.Items)
                    {
                        var rowVm = new PurchaseEntryItemRowViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ProductSKU = item.ProductSKU,
                            BatchNo = item.BatchNo,
                            ExpiryDate = item.ExpiryDate,
                            Quantity = item.Quantity,
                            CostPrice = item.CostPrice,
                            SellingPrice = item.SellingPrice,
                            MRP = item.MRP,
                            TaxAmount = item.TaxAmount
                        };
                        rowVm.PropertyChanged += ItemRow_PropertyChanged;
                        Items.Add(rowVm);
                    }

                    CalculateTotals();

                    // Load pending POs for this supplier
                    await LoadPendingPurchaseOrdersAsync(SupplierId);
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load purchase entry.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// IMPORT FROM PURCHASE ORDER - Key feature for rapid data entry
        /// </summary>
        private async Task ImportFromPOAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            try
            {
                IsBusy = true;

                // Confirm with user
                var result = MessageBox.Show(
                    $"Import items from Purchase Order: {SelectedPurchaseOrder.ReferenceNo}?\n\n" +
                    $"This will add {SelectedPurchaseOrder.Items.Count} item(s) to this entry.",
                    "Import from PO",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Set supplier and PO reference
                    SupplierId = SelectedPurchaseOrder.SupplierId;
                    PurchaseOrderId = SelectedPurchaseOrder.PurchaseOrderId;

                    // Import all items from PO
                    foreach (var poItem in SelectedPurchaseOrder.Items)
                    {
                        // Check if product already exists in items
                        var existing = Items.FirstOrDefault(i => i.ProductId == poItem.ProductId);
                        if (existing != null)
                        {
                            // Update quantity
                            existing.Quantity += poItem.Quantity;
                        }
                        else
                        {
                            // Add new item
                            var rowVm = new PurchaseEntryItemRowViewModel
                            {
                                ProductId = poItem.ProductId,
                                ProductName = poItem.ProductName,
                                ProductSKU = poItem.ProductSKU,
                                Quantity = poItem.Quantity,
                                CostPrice = poItem.UnitPrice,
                                SellingPrice = poItem.UnitPrice * 1.2m, // 20% markup default
                                MRP = poItem.UnitPrice * 1.3m, // 30% markup default
                                TaxAmount = poItem.TaxAmount,
                                BatchNo = string.Empty,
                                ExpiryDate = null
                            };
                            rowVm.PropertyChanged += ItemRow_PropertyChanged;
                            Items.Add(rowVm);
                        }
                    }

                    CalculateTotals();

                    POS.UI.Components.DialogService.Success("Success", 
                        $"Imported {SelectedPurchaseOrder.Items.Count} item(s) from Purchase Order.\n\n" +
                        "Please update Batch No, Expiry Date, and Prices as needed.");
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to import from PO.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SearchProductsAsync(string keyword)
        {
            try
            {
                var productService = App.ServiceProvider?.GetService(typeof(ProductApiService)) as ProductApiService;
                if (productService != null && !string.IsNullOrWhiteSpace(keyword))
                {
                    var results = await productService.SearchAsync(keyword);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProductSearchResults.Clear();
                        foreach (var product in results.Take(10))
                        {
                            ProductSearchResults.Add(product);
                        }
                        IsProductSearchPopupOpen = ProductSearchResults.Count > 0;
                    });
                }
                else
                {
                    ProductSearchResults.Clear();
                    IsProductSearchPopupOpen = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Product search error: {ex.Message}");
                IsProductSearchPopupOpen = false;
            }
        }

        private void AddProductToItems(ProductDto product)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity += 1;
                return;
            }

            var rowVm = new PurchaseEntryItemRowViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                Quantity = 1,
                CostPrice = product.CostPrice,
                SellingPrice = product.SellingPrice,
                MRP = product.MRP,
                TaxAmount = 0,
                BatchNo = string.Empty,
                ExpiryDate = null
            };
            rowVm.PropertyChanged += ItemRow_PropertyChanged;
            Items.Add(rowVm);
            CalculateTotals();
        }

        private void ShowProductSearch()
        {
            // Focus handled in view
        }

        private void RemoveItem()
        {
            if (SelectedItem != null)
            {
                SelectedItem.PropertyChanged -= ItemRow_PropertyChanged;
                Items.Remove(SelectedItem);
                CalculateTotals();
            }
        }

        private void ItemRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PurchaseEntryItemRowViewModel.Total) ||
                e.PropertyName == nameof(PurchaseEntryItemRowViewModel.TaxAmount))
            {
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            TaxAmount = Items.Sum(i => i.TaxAmount);
            TotalAmount = Items.Sum(i => i.Total);
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return SupplierId != Guid.Empty && Items.Count > 0 && !string.IsNullOrWhiteSpace(InvoiceNo);
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!Validate())
                    return;

                IsBusy = true;

                var dto = new CreatePurchaseEntryDto
                {
                    SupplierId = SupplierId,
                    PurchaseOrderId = PurchaseOrderId,
                    InvoiceNo = InvoiceNo,
                    InvoiceDate = InvoiceDate,
                    ReceivedDate = ReceivedDate,
                    Notes = Notes,
                    Items = Items.Select(i => new CreatePurchaseEntryItemDto
                    {
                        ProductId = i.ProductId,
                        BatchNo = i.BatchNo,
                        ExpiryDate = i.ExpiryDate,
                        Quantity = i.Quantity,
                        CostPrice = i.CostPrice,
                        SellingPrice = i.SellingPrice,
                        MRP = i.MRP,
                        TaxAmount = i.TaxAmount
                    }).ToList()
                };

                if (_purchaseEntryId.HasValue)
                {
                    await _service.UpdateAsync(_purchaseEntryId.Value, dto);
                }
                else
                {
                    await _service.CreateAsync(dto);
                }

                // Close window first, then notify and show message
                CloseWindow();
                OnSaved?.Invoke();
                
                // Show success message after window is closed
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_purchaseEntryId.HasValue)
                    {
                        POS.UI.Components.DialogService.Success("Success", "Purchase entry updated successfully.");
                    }
                    else
                    {
                        POS.UI.Components.DialogService.Success("Success", 
                            "Purchase entry created successfully!\n\n" +
                            "Remember to PROCESS this entry to update inventory.");
                    }
                }));
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to save purchase entry.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool Validate()
        {
            if (SupplierId == Guid.Empty)
            {
                POS.UI.Components.DialogService.Warning("Validation Error", "Please select a supplier.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(InvoiceNo))
            {
                POS.UI.Components.DialogService.Warning("Validation Error", "Invoice number is required.");
                return false;
            }

            if (Items.Count == 0)
            {
                POS.UI.Components.DialogService.Warning("Validation Error", "Please add at least one item.");
                return false;
            }

            foreach (var item in Items)
            {
                if (item.Quantity <= 0)
                {
                    POS.UI.Components.DialogService.Warning("Validation Error", 
                        $"Quantity must be greater than zero for {item.ProductName}.");
                    return false;
                }
                if (item.CostPrice < 0)
                {
                    POS.UI.Components.DialogService.Warning("Validation Error", 
                        $"Cost price cannot be negative for {item.ProductName}.");
                    return false;
                }
            }

            return true;
        }

        private void CloseWindow()
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }

    // ================= ITEM ROW VIEW MODEL =================

    public class PurchaseEntryItemRowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }

        private string _batchNo;
        public string BatchNo
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
                OnPropertyChanged(nameof(Total));
            }
        }

        private decimal _costPrice;
        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                _costPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        private decimal _sellingPrice;
        public decimal SellingPrice
        {
            get => _sellingPrice;
            set { _sellingPrice = value; OnPropertyChanged(); }
        }

        private decimal _mrp;
        public decimal MRP
        {
            get => _mrp;
            set { _mrp = value; OnPropertyChanged(); }
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set
            {
                _taxAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => (Quantity * CostPrice) + TaxAmount;
    }
}
