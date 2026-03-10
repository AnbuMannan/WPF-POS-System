using POS.Shared.Models;
using POS.Shared.Enums;
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

namespace POS.UI.Modules.Suppliers.PurchaseOrder
{
    public class CreatePurchaseOrderViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly PurchaseOrderApiService _service;
        private readonly Guid? _purchaseOrderId;
        private readonly bool _isReadOnly;

        public event Action? OnSaved;

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
            set { _supplierId = value; OnPropertyChanged(); }
        }

        private SupplierDto? _selectedSupplier;
        public SupplierDto? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                
                // Update related properties when supplier is selected
                if (value != null)
                {
                    SupplierId = value.Id;
                    SupplierName = value.Name;
                }
                else
                {
                    SupplierId = Guid.Empty;
                    SupplierName = string.Empty;
                }
                
                // Force Save command to re-evaluate
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        private string _supplierName = string.Empty;
        public string SupplierName
        {
            get => _supplierName;
            set { _supplierName = value; OnPropertyChanged(); }
        }

        private DateTime _orderDate = DateTime.Now;
        public DateTime OrderDate
        {
            get => _orderDate;
            set { _orderDate = value; OnPropertyChanged(); }
        }

        private DateTime? _expectedDeliveryDate;
        public DateTime? ExpectedDeliveryDate
        {
            get => _expectedDeliveryDate;
            set { _expectedDeliveryDate = value; OnPropertyChanged(); }
        }

        private string _referenceNo = string.Empty;
        public string ReferenceNo
        {
            get => _referenceNo;
            set { _referenceNo = value; OnPropertyChanged(); }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private PurchaseOrderStatus _status = PurchaseOrderStatus.Draft;
        public PurchaseOrderStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // ================= SUPPLIERS LIST (FOR DROPDOWN) =================

        public ObservableCollection<SupplierDto> Suppliers { get; set; } = new();

        // ================= ITEMS COLLECTION =================

        public ObservableCollection<PurchaseOrderItemRowViewModel> Items { get; set; } = new();

        private PurchaseOrderItemRowViewModel? _selectedItem;
        public PurchaseOrderItemRowViewModel? SelectedItem
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

        // ================= PRODUCT SEARCH =================

        private string _productSearchText = string.Empty;
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

        private ProductDto? _selectedProduct;
        public ProductDto? SelectedProduct
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
        public ICommand FocusProductSearchCommand { get; }

        // ================= CONSTRUCTOR =================

        public CreatePurchaseOrderViewModel(PurchaseOrderApiService service, Guid? purchaseOrderId = null, bool isReadOnly = false)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _purchaseOrderId = purchaseOrderId;
            _isReadOnly = isReadOnly;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isReadOnly && CanSave());
            CancelCommand = new RelayCommand(() => CloseWindow());
            AddItemCommand = new RelayCommand(() => ShowProductSearch(), () => !_isReadOnly);
            RemoveItemCommand = new RelayCommand(() => RemoveItem(), () => !_isReadOnly && SelectedItem != null);
            LoadSuppliersCommand = new RelayCommand(async () => await LoadSuppliersAsync());
            FocusProductSearchCommand = new RelayCommand(() => { /* Will be handled in view */ });

            InitializeViewModelAsync(); // Call the async initializer
        }

        // Async initializer to handle async operations in constructor
        private async void InitializeViewModelAsync()
        {
            try
            {
                await LoadSuppliersAsync();

                if (_purchaseOrderId.HasValue)
                {
                    await LoadPurchaseOrderAsync(_purchaseOrderId.Value);
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize purchase order view: {ex.Message}");
            }
        }

        // ================= METHODS =================

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var supplierService = App.ServiceProvider?.GetService(typeof(SupplierApiService)) as SupplierApiService;
                if (supplierService == null)
                {
                    POS.UI.Components.DialogService.Warning("Error", "Supplier service not available.");
                    return;
                }
                
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
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load suppliers.\n{ex.Message}");
            }
        }

        private async Task LoadPurchaseOrderAsync(Guid id)
        {
            try
            {
                IsBusy = true;
                var po = await _service.GetByIdAsync(id);
                if (po != null)
                {
                    SupplierId = po.SupplierId;
                    SupplierName = po.SupplierName;
                    OrderDate = po.OrderDate;
                    ExpectedDeliveryDate = po.ExpectedDeliveryDate;
                    ReferenceNo = po.ReferenceNo;
                    Notes = po.Notes;
                    Status = po.Status;

                    Items.Clear();
                    foreach (var item in po.Items)
                    {
                        var rowVm = new PurchaseOrderItemRowViewModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ProductSKU = item.ProductSKU,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TaxAmount = item.TaxAmount
                        };
                        rowVm.PropertyChanged += ItemRow_PropertyChanged;
                        Items.Add(rowVm);
                    }

                    CalculateTotals();
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load purchase order.\n{ex.Message}");
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
                if (productService == null)
                {
                    System.Diagnostics.Debug.WriteLine("Product service not available.");
                    IsProductSearchPopupOpen = false;
                    return;
                }
                
                if (!string.IsNullOrWhiteSpace(keyword))
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
                // Silent fail for search
                System.Diagnostics.Debug.WriteLine($"Product search error: {ex.Message}");
                IsProductSearchPopupOpen = false;
            }
        }

        private void AddProductToItems(ProductDto product)
        {
            // Check if product already exists
            var existing = Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity += 1;
                return;
            }

            var rowVm = new PurchaseOrderItemRowViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                Quantity = 1,
                UnitPrice = product.CostPrice,
                TaxAmount = 0
            };
            rowVm.PropertyChanged += ItemRow_PropertyChanged;
            Items.Add(rowVm);
            CalculateTotals();
        }

        private void ShowProductSearch()
        {
            // Focus on product search textbox
            // This would be handled in the view
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
            if (e.PropertyName == nameof(PurchaseOrderItemRowViewModel.Total) ||
                e.PropertyName == nameof(PurchaseOrderItemRowViewModel.Quantity) ||
                e.PropertyName == nameof(PurchaseOrderItemRowViewModel.UnitPrice) ||
                e.PropertyName == nameof(PurchaseOrderItemRowViewModel.TaxAmount))
            {
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            TotalAmount = Items.Sum(i => i.Total);
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return SupplierId != Guid.Empty && Items.Count > 0;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!Validate())
                    return;

                IsBusy = true;

                var dto = new CreatePurchaseOrderDto
                {
                    SupplierId = SupplierId,
                    OrderDate = OrderDate,
                    ExpectedDeliveryDate = ExpectedDeliveryDate,
                    ReferenceNo = ReferenceNo,
                    Notes = Notes,
                    Items = Items.Select(i => new CreatePurchaseOrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TaxAmount = i.TaxAmount
                    }).ToList()
                };

                if (_purchaseOrderId.HasValue)
                {
                    await _service.UpdateAsync(_purchaseOrderId.Value, dto);
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
                    if (_purchaseOrderId.HasValue)
                    {
                        POS.UI.Components.DialogService.Success("Success", "Purchase order updated successfully.");
                    }
                    else
                    {
                        POS.UI.Components.DialogService.Success("Success", "Purchase order created successfully.");
                    }
                }));
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to save purchase order.\n{ex.Message}");
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

            if (Items.Count == 0)
            {
                POS.UI.Components.DialogService.Warning("Validation Error", "Please add at least one item.");
                return false;
            }

            foreach (var item in Items)
            {
                if (item.Quantity <= 0)
                {
                    POS.UI.Components.DialogService.Warning("Validation Error", $"Quantity must be greater than zero for {item.ProductName}.");
                    return false;
                }
                if (item.UnitPrice < 0)
                {
                    POS.UI.Components.DialogService.Warning("Validation Error", $"Unit price cannot be negative for {item.ProductName}.");
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

    public class PurchaseOrderItemRowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }

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

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => (Quantity * UnitPrice) + TaxAmount;
    }
}
