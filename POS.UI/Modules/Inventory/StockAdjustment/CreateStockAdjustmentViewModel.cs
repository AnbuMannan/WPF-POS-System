using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.Modules.Inventory.StockAdjustment
{
    public class CreateStockAdjustmentViewModel : ViewModelBase
    {
        private readonly StockAdjustmentApiService _adjustmentService;
        private readonly ProductApiService _productService;
        private readonly StockApiService _stockService;

        // ================= COLLECTIONS =================

        public ObservableCollection<AdjustmentItemRow> Items { get; set; } = new();
        public ObservableCollection<ProductDto> Products { get; set; } = new();
        public ObservableCollection<ProductDto> FilteredProducts { get; set; } = new();

        public List<string> Reasons { get; } = new List<string>(AdjustmentReasons.All);

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

        private DateTime _adjustmentDate = DateTime.Now;
        public DateTime AdjustmentDate
        {
            get => _adjustmentDate;
            set
            {
                _adjustmentDate = value;
                OnPropertyChanged();
            }
        }

        private string _selectedReason = AdjustmentReasons.Damage;
        public string SelectedReason
        {
            get => _selectedReason;
            set
            {
                _selectedReason = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStockReduction));
                OnPropertyChanged(nameof(ReasonHelpText));
                ValidateAllItems();
            }
        }

        private string _remarks = string.Empty;
        public string Remarks
        {
            get => _remarks;
            set
            {
                _remarks = value;
                OnPropertyChanged();
            }
        }

        public bool IsStockReduction => SelectedReason is AdjustmentReasons.Damage 
            or AdjustmentReasons.Theft 
            or AdjustmentReasons.Expiry;

        public string ReasonHelpText => SelectedReason switch
        {
            AdjustmentReasons.Damage => "Enter quantities to remove from stock due to damage",
            AdjustmentReasons.Theft => "Enter quantities to remove from stock due to theft/loss",
            AdjustmentReasons.Expiry => "Enter quantities to remove from stock due to expiry",
            AdjustmentReasons.Correction => "Enter positive or negative quantities to correct stock counts",
            _ => "Enter quantities to adjust"
        };

        // ================= PRODUCT SEARCH =================

        private string _productSearchText = string.Empty;
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                FilterProducts();
                // Show popup when typing
                IsProductSearchPopupOpen = !string.IsNullOrEmpty(value) && FilteredProducts.Count > 0;
            }
        }

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
                // Auto-add when product is selected from popup
                if (value != null)
                {
                    IsProductSearchPopupOpen = false;
                    AddItem();
                }
            }
        }

        private AdjustmentItemRow? _selectedItem;
        public AdjustmentItemRow? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        // ================= TOTALS =================

        public decimal TotalValue => Items.Sum(i => Math.Abs(i.TotalValue));
        public int TotalItems => Items.Count;

        // ================= VALIDATION =================

        private string? _validationError;
        public string? ValidationError
        {
            get => _validationError;
            set
            {
                _validationError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidationError));
            }
        }

        public bool HasValidationError => !string.IsNullOrEmpty(ValidationError);

        // ================= COMMANDS =================

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand RemoveSelectedItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand CancelCommand { get; }

        // ================= EVENTS =================

        public event Action? RequestClose;
        public event Action<StockAdjustmentDto>? AdjustmentSaved;

        // ================= CONSTRUCTOR =================

        public CreateStockAdjustmentViewModel(
            StockAdjustmentApiService adjustmentService,
            ProductApiService productService,
            StockApiService stockService)
        {
            _adjustmentService = adjustmentService;
            _productService = productService;
            _stockService = stockService;

            AddItemCommand = new RelayCommand(AddItem, () => SelectedProduct != null);
            RemoveItemCommand = new RelayCommand<AdjustmentItemRow>(RemoveItem);
            RemoveSelectedItemCommand = new RelayCommand(() => RemoveItem(SelectedItem), () => SelectedItem != null);
            SaveCommand = new RelayCommand(async () => await SaveAsync(), CanSave);
            ClearCommand = new RelayCommand(Clear);
            CancelCommand = new RelayCommand(Cancel);

            _ = LoadProductsAsync();
        }

        // ================= LOAD DATA =================

        private async Task LoadProductsAsync()
        {
            try
            {
                IsBusy = true;
                var products = await _productService.GetAllAsync();
                Products.Clear();
                FilteredProducts.Clear();
                foreach (var p in products.Where(p => p.IsActive))
                {
                    Products.Add(p);
                    FilteredProducts.Add(p);
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Load Failed", $"Failed to load products: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterProducts()
        {
            FilteredProducts.Clear();
            var search = ProductSearchText?.ToLower() ?? "";

            foreach (var p in Products.Where(p =>
                string.IsNullOrEmpty(search) ||
                p.Name.ToLower().Contains(search) ||
                (p.SKU?.ToLower().Contains(search) ?? false) ||
                (p.Barcode?.ToLower().Contains(search) ?? false)))
            {
                FilteredProducts.Add(p);
            }
        }

        // ================= ITEM MANAGEMENT =================

        private async void AddItem()
        {
            if (SelectedProduct == null)
                return;

            // Check if product already exists in items
            var existing = Items.FirstOrDefault(i => i.ProductId == SelectedProduct.ProductId);
            if (existing != null)
            {
                POS.UI.Components.DialogService.Warning("Duplicate", 
                    $"'{SelectedProduct.Name}' is already in the list. Modify the existing row.");
                return;
            }

            try
            {
                // Get current stock
                decimal currentStock = 0;
                try
                {
                    currentStock = await _stockService.GetProductStockAsync(SelectedProduct.ProductId);
                }
                catch
                {
                    // If stock service fails, continue with 0
                }

                var item = new AdjustmentItemRow
                {
                    ProductId = SelectedProduct.ProductId,
                    ProductName = SelectedProduct.Name,
                    ProductSku = SelectedProduct.SKU,
                    CurrentStock = currentStock,
                    Quantity = IsStockReduction ? -1 : 1, // Default to -1 for reductions, +1 for corrections
                    CostPrice = SelectedProduct.CostPrice
                };

                item.PropertyChanged += Item_PropertyChanged;
                Items.Add(item);

                // Clear search
                ProductSearchText = string.Empty;
                SelectedProduct = null;

                UpdateTotals();
                ValidateAllItems();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Error", $"Failed to add item: {ex.Message}");
            }
        }

        private void RemoveItem(AdjustmentItemRow? item)
        {
            if (item == null)
                return;

            item.PropertyChanged -= Item_PropertyChanged;
            Items.Remove(item);
            UpdateTotals();
            ValidateAllItems();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AdjustmentItemRow.Quantity) || 
                e.PropertyName == nameof(AdjustmentItemRow.CostPrice))
            {
                UpdateTotals();
                ValidateAllItems();
            }
        }

        private void UpdateTotals()
        {
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(TotalItems));
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        // ================= VALIDATION =================

        private void ValidateAllItems()
        {
            ValidationError = null;

            foreach (var item in Items)
            {
                item.ValidationError = null;

                // For stock reduction, quantity should be negative and not exceed current stock
                if (IsStockReduction)
                {
                    var absQty = Math.Abs(item.Quantity);
                    if (absQty > item.CurrentStock)
                    {
                        item.ValidationError = $"Exceeds available stock ({item.CurrentStock:N2})";
                        ValidationError = $"Some items exceed available stock";
                    }
                }
            }

            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return Items.Count > 0 && 
                   !HasValidationError && 
                   Items.All(i => i.Quantity != 0 && string.IsNullOrEmpty(i.ValidationError));
        }

        // ================= SAVE =================

        private async Task SaveAsync()
        {
            if (!CanSave())
                return;

            try
            {
                IsBusy = true;

                var dto = new CreateStockAdjustmentDto
                {
                    AdjustmentDate = AdjustmentDate,
                    AdjustedBy = Environment.UserName,
                    Reason = SelectedReason,
                    Remarks = Remarks,
                    Items = Items.Select(i => new CreateStockAdjustmentItemDto
                    {
                        ProductId = i.ProductId,
                        BatchNo = i.BatchNo,
                        Quantity = IsStockReduction ? -Math.Abs(i.Quantity) : i.Quantity, // Ensure negative for reductions
                        CostPrice = i.CostPrice,
                        Remarks = i.Remarks
                    }).ToList()
                };

                // Validate before saving
                var (isValid, errorMessage) = await _adjustmentService.ValidateStockAsync(dto);
                if (!isValid)
                {
                    POS.UI.Components.DialogService.Error("Validation Failed", errorMessage ?? "Stock validation failed");
                    return;
                }

                var result = await _adjustmentService.CreateAndApproveAsync(dto, Environment.UserName);

                POS.UI.Components.DialogService.Success("Success", 
                    $"Stock adjustment '{result.ReferenceNo}' saved and processed successfully.");

                AdjustmentSaved?.Invoke(result);
                Clear();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Save Failed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ================= CLEAR =================

        private void Clear()
        {
            AdjustmentDate = DateTime.Now;
            SelectedReason = AdjustmentReasons.Damage;
            Remarks = string.Empty;
            ProductSearchText = string.Empty;
            SelectedProduct = null;
            SelectedItem = null;
            ValidationError = null;
            IsProductSearchPopupOpen = false;

            foreach (var item in Items)
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }
            Items.Clear();

            UpdateTotals();
        }

        // ================= CANCEL =================

        private void Cancel()
        {
            // If there are unsaved items, ask for confirmation
            if (Items.Count > 0)
            {
                var result = POS.UI.Components.DialogService.Confirm(
                    "Discard Changes?",
                    "You have unsaved items. Are you sure you want to go back?");
                
                if (result != System.Windows.MessageBoxResult.Yes)
                    return;
            }

            RequestClose?.Invoke();
        }

        // ================= KEYBOARD SHORTCUTS =================

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F2 && CanSave())
            {
                _ = SaveAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                Cancel();
                e.Handled = true;
            }
        }
    }

    // ================= ITEM ROW MODEL =================

    public class AdjustmentItemRow : INotifyPropertyChanged
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductSku { get; set; }
        public string? BatchNo { get; set; }

        private decimal _currentStock;
        public decimal CurrentStock
        {
            get => _currentStock;
            set
            {
                _currentStock = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentStock)));
            }
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValue)));
            }
        }

        private decimal _costPrice;
        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                _costPrice = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CostPrice)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalValue)));
            }
        }

        public decimal TotalValue => Math.Abs(Quantity) * CostPrice;

        public string? Remarks { get; set; }

        private string? _validationError;
        public string? ValidationError
        {
            get => _validationError;
            set
            {
                _validationError = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValidationError)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasError)));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ValidationError);

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
