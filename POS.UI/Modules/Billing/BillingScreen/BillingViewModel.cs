using System.Collections.ObjectModel;
using System.Windows.Input;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Models;
using POS.Shared.Models;
using POS.UI.Modules.Billing.ProductSearch;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS.UI.Modules.Billing.BillingScreen
{
    public class BillingViewModel : ViewModelBase
    {
        private readonly BillingApiService _billingApi;
        private readonly ProductApiService _productApi;
        private readonly CustomerApiService _customerApi;
        private readonly TaxProfileApiService? _taxProfileApi;
        private readonly UomApiService? _uomApi;
        private readonly IPrintService? _printService;
        private readonly IPrintSettingsService? _printSettings;
        private readonly AuditLogApiService? _auditLogApi;
        private readonly LoyaltyApiService _loyaltyApi;

        private ObservableCollection<CartItem> _cartItems = new();
        private CartItem? _selectedCartItem;
        private bool _isCartEmpty = true;
        private decimal _subtotal;
        private decimal _discountAmount;
        private decimal _totalTax;
        private decimal _grandTotal;
        private string? _billNumber;
        private DateTime _billDate = DateTime.Now;
        private ObservableCollection<CustomerDto> _customers = new();
        private CustomerDto? _selectedCustomer;
        private int _availableLoyaltyPoints;
        private int _pointsToRedeem;
        private decimal _loyaltyRedemptionAmount;
        private bool _isDiscountByPercent = true;
        private string _discountInputValue = string.Empty;
        private BillSummary _billSummary = new();
        private string _statusMessage = string.Empty;
        private bool _isStatusError;
        private bool _isBusy;
        private ProductDto? _selectedSearchProduct;

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set { _cartItems = value; OnPropertyChanged(); }
        }

        public CartItem? SelectedCartItem
        {
            get => _selectedCartItem;
            set { _selectedCartItem = value; OnPropertyChanged(); }
        }

        public bool IsCartEmpty
        {
            get => _isCartEmpty;
            set { _isCartEmpty = value; OnPropertyChanged(); }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); }
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set { _discountAmount = value; OnPropertyChanged(); }
        }

        public decimal TotalTax
        {
            get => _totalTax;
            set { _totalTax = value; OnPropertyChanged(); }
        }

        public decimal GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        public string? BillNumber
        {
            get => _billNumber;
            set { _billNumber = value; OnPropertyChanged(); }
        }

        public DateTime BillDate
        {
            get => _billDate;
            set { _billDate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CustomerDto> Customers
        {
            get => _customers;
            set { _customers = value; OnPropertyChanged(); }
        }

        public CustomerDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                AvailableLoyaltyPoints = _selectedCustomer?.LoyaltyPoints ?? 0;
                (RedeemPointsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public int AvailableLoyaltyPoints
        {
            get => _availableLoyaltyPoints;
            set { _availableLoyaltyPoints = value; OnPropertyChanged(); }
        }

        public int PointsToRedeem
        {
            get => _pointsToRedeem;
            set
            {
                _pointsToRedeem = value;
                OnPropertyChanged();
                (RedeemPointsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public decimal LoyaltyRedemptionAmount
        {
            get => _loyaltyRedemptionAmount;
            set { _loyaltyRedemptionAmount = value; OnPropertyChanged(); }
        }

        public bool IsDiscountByPercent
        {
            get => _isDiscountByPercent;
            set { _isDiscountByPercent = value; OnPropertyChanged(); }
        }

        public string DiscountInputValue
        {
            get => _discountInputValue;
            set { _discountInputValue = value; OnPropertyChanged(); }
        }

        public BillSummary BillSummary
        {
            get => _billSummary;
            set { _billSummary = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsStatusError
        {
            get => _isStatusError;
            set { _isStatusError = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ProductDto? SelectedSearchProduct
        {
            get => _selectedSearchProduct;
            set { _selectedSearchProduct = value; OnPropertyChanged(); }
        }

        public ProductSearchViewModel ProductSearch { get; }

        // Commands
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand ApplyDiscountCommand { get; }
        public ICommand ProceedToPaymentCommand { get; }
        public ICommand LoadCustomersCommand { get; }
        public ICommand GenerateBillNumberCommand { get; }
        public ICommand FocusSearchCommand { get; }
        public ICommand FocusCustomerCommand { get; }
        public ICommand FocusDiscountCommand { get; }
        public ICommand OpenHoldBillDialogCommand { get; }
        public ICommand OpenDraftBillDialogCommand { get; }
        public ICommand ShowHeldBillsCommand { get; }
        public ICommand ShowDraftBillsCommand { get; }
        public ICommand ReprintLastReceiptCommand { get; }
        public ICommand NewBillCommand { get; }
        public ICommand ShowShortcutsCommand { get; }
        public ICommand ToggleFullScreenCommand { get; }
        public ICommand RedeemPointsCommand { get; }

        // Actions (set by View)
        public Func<decimal, Task<List<PaymentDto>?>>? OpenPaymentDialogAsync { get; set; }
        public Action? OpenHoldBillDialog { get; set; }
        public Action? OpenDraftBillDialog { get; set; }
        public Action? ShowHeldBills { get; set; }
        public Action? ShowDraftBills { get; set; }
        public Action<ReceiptDto>? ShowReceiptPreview { get; set; }
        public Action? FocusSearchRequested { get; set; }
        public Action? FocusCustomerRequested { get; set; }
        public Action? FocusDiscountRequested { get; set; }
        public Action? ToggleQuickProductsRequested { get; set; }
        public Action? ShowShortcutsRequested { get; set; }
        public Action? ReprintReceiptRequested { get; set; }
        public Action? RequestNewBill { get; set; }
        public Action? ToggleFullScreenRequested { get; set; }

        public BillingViewModel(
            BillingApiService billingApi,
            ProductApiService productApi,
            CustomerApiService customerApi,
            TaxProfileApiService? taxProfileApi,
            UomApiService? uomApi,
            IPrintService? printService,
            IPrintSettingsService? printSettings,
            AuditLogApiService? auditLogApi,
            LoyaltyApiService loyaltyApi)
        {
            _billingApi = billingApi;
            _productApi = productApi;
            _customerApi = customerApi;
            _taxProfileApi = taxProfileApi;
            _uomApi = uomApi;
            _printService = printService;
            _printSettings = printSettings;
            _auditLogApi = auditLogApi;
            _loyaltyApi = loyaltyApi;

            ProductSearch = new ProductSearchViewModel();
            ProductSearch.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(ProductSearch.SearchText) && !string.IsNullOrWhiteSpace(ProductSearch.SearchText))
                {
                    await SearchProducts(ProductSearch.SearchText);
                }
            };
            
            // Initialize commands
            RemoveFromCartCommand = new RelayCommand<object?>(RemoveFromCart, _ => SelectedCartItem != null);
            ClearCartCommand = new RelayCommand(() => ClearCart());
            ApplyDiscountCommand = new RelayCommand(() => ApplyDiscount());
            ProceedToPaymentCommand = new RelayCommand(async () => await ProceedToPayment(), () => CartItems.Count > 0);
            LoadCustomersCommand = new RelayCommand(async () => await LoadCustomers());
            GenerateBillNumberCommand = new RelayCommand(async () => await GenerateBillNumber());
            FocusSearchCommand = new RelayCommand(() => FocusSearchRequested?.Invoke());
            FocusCustomerCommand = new RelayCommand(() => FocusCustomerRequested?.Invoke());
            FocusDiscountCommand = new RelayCommand(() => FocusDiscountRequested?.Invoke());
            OpenHoldBillDialogCommand = new RelayCommand(() => OpenHoldBillDialog?.Invoke());
            OpenDraftBillDialogCommand = new RelayCommand(() => OpenDraftBillDialog?.Invoke());
            ShowHeldBillsCommand = new RelayCommand(() => ShowHeldBills?.Invoke());
            ShowDraftBillsCommand = new RelayCommand(() => ShowDraftBills?.Invoke());
            ReprintLastReceiptCommand = new RelayCommand(() => ReprintReceiptRequested?.Invoke());
            NewBillCommand = new RelayCommand(() => RequestNewBill?.Invoke());
            ShowShortcutsCommand = new RelayCommand(() => ShowShortcutsRequested?.Invoke());
            ToggleFullScreenCommand = new RelayCommand(() => ToggleFullScreenRequested?.Invoke());
            RedeemPointsCommand = new RelayCommand(async () => await RedeemPointsAsync(), () => SelectedCustomer != null && PointsToRedeem > 0);

            // Subscribe to cart changes
            CartItems.CollectionChanged += (s, e) =>
            {
                IsCartEmpty = CartItems.Count == 0;
                RecalculateTotals();
                (ProceedToPaymentCommand as RelayCommand)?.RaiseCanExecuteChanged();
            };
        }

        public async Task SearchProducts(string searchText)
        {
            try
            {
                var results = await _productApi.SearchAsync(searchText);
                if (results != null && results.Count > 0)
                {
                    // Fetch stock for each result using the global ServiceProvider
                    var stockApiService = App.ServiceProvider?.GetService(typeof(StockApiService)) as StockApiService;
                    if (stockApiService != null)
                    {
                        foreach (var product in results)
                        {
                            product.AvailableStock = await stockApiService.GetProductStockAsync(product.ProductId);
                        }
                    }
                }
                ProductSearch.SearchResults = new ObservableCollection<ProductDto>(results ?? new List<ProductDto>());
            }
            catch (Exception ex)
            {
                SetError($"Search failed: {ex.Message}");
            }
        }

        public void AddProductToCart(ProductDto product, decimal quantity = 1)
        {
            var productGuid = LongToGuid(product.ProductId);
            var existing = CartItems.FirstOrDefault(x => x.ProductId == productGuid);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                // Get tax rate from tax profile (will need to be loaded separately)
                decimal taxRate = 18m; // Default GST rate - TODO: Load from TaxProfile
                
                var cartItem = new CartItem
                {
                    ProductId = productGuid,
                    LineNumber = CartItems.Count + 1,
                    SKU = product.SKU ?? string.Empty,
                    ProductName = product.Name ?? string.Empty,
                    HSNCode = product.HSNCode ?? string.Empty,
                    Quantity = quantity,
                    UnitName = product.Unit ?? "PCS",
                    UnitId = null, // Not directly available in ProductDto
                    MRP = product.MRP,
                    ActualPrice = product.SellingPrice,
                    TaxRate = taxRate,
                    TaxProfileId = product.TaxProfileId > 0 ? LongToGuid(product.TaxProfileId) : null,
                    TaxProfileIdValue = product.TaxProfileId > 0 ? product.TaxProfileId : 0
                };

                cartItem.PropertyChanged += (s, e) => RecalculateTotals();
                CartItems.Add(cartItem);
                cartItem.RefreshTotals(); // Ensure tax/totals computed after full init
            }

            RecalculateTotals();
        }

        /// <summary>Converts a long product ID to a deterministic Guid for cart/API compatibility.</summary>
        private static Guid LongToGuid(long value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        private void RemoveFromCart(object? parameter)
        {
            if (parameter is CartItem item)
            {
                CartItems.Remove(item);
                RenumberCartItems();
            }
        }

        private void ClearCart()
        {
            CartItems.Clear();
            BillNumber = null;
            SelectedCustomer = null;
            DiscountInputValue = string.Empty;
            StatusMessage = string.Empty;
            PointsToRedeem = 0;
            LoyaltyRedemptionAmount = 0;
            AvailableLoyaltyPoints = 0;
        }

        private void RenumberCartItems()
        {
            for (int i = 0; i < CartItems.Count; i++)
            {
                CartItems[i].LineNumber = i + 1;
            }
        }

        private void ApplyDiscount()
        {
            if (string.IsNullOrWhiteSpace(DiscountInputValue)) return;

            if (!decimal.TryParse(DiscountInputValue, out var discountValue) || discountValue < 0)
            {
                SetError("Invalid discount value");
                return;
            }

            if (IsDiscountByPercent)
            {
                if (discountValue > 100)
                {
                    SetError("Discount percent cannot exceed 100%");
                    return;
                }

                foreach (var item in CartItems)
                {
                    item.DiscountPercent = discountValue;
                }
            }
            else
            {
                // Apply fixed discount proportionally
                var subtotal = CartItems.Sum(x => x.Quantity * x.ActualPrice);
                if (discountValue > subtotal)
                {
                    SetError("Discount amount cannot exceed subtotal");
                    return;
                }

                foreach (var item in CartItems)
                {
                    var itemSubtotal = item.Quantity * item.ActualPrice;
                    var itemDiscountPercent = (discountValue / subtotal) * 100;
                    item.DiscountPercent = itemDiscountPercent;
                }
            }

            RecalculateTotals();
            SetSuccess("Discount applied");
        }

        private void RecalculateTotals()
        {
            Subtotal = CartItems.Sum(x => x.Quantity * x.ActualPrice);
            DiscountAmount = CartItems.Sum(x => x.DiscountAmount);
            TotalTax = CartItems.Sum(x => x.TaxAmount);
            var baseGrandTotal = CartItems.Sum(x => x.TotalAmount);
            var adjustedGrandTotal = baseGrandTotal - LoyaltyRedemptionAmount;
            if (adjustedGrandTotal < 0) adjustedGrandTotal = 0;
            GrandTotal = adjustedGrandTotal;

            // Update BillSummary
            var taxableAmount = Subtotal - DiscountAmount;
            BillSummary.Subtotal = Subtotal;
            BillSummary.DiscountAmount = DiscountAmount;
            BillSummary.CGST = TotalTax / 2;
            BillSummary.SGST = TotalTax / 2;
            BillSummary.IGST = 0;
            BillSummary.RoundOff = Math.Round(GrandTotal) - GrandTotal;
            BillSummary.GrandTotal = Math.Round(GrandTotal);
        }

        private async Task ProceedToPayment()
        {
            if (CartItems.Count == 0)
            {
                SetError("Cart is empty");
                return;
            }

            if (OpenPaymentDialogAsync == null)
            {
                SetError("Payment dialog not initialized");
                return;
            }

            try
            {
                IsBusy = true;
                var payments = await OpenPaymentDialogAsync(BillSummary.GrandTotal);
                
                if (payments != null && payments.Count > 0)
                {
                    await CompleteSale(payments);
                }
            }
            catch (Exception ex)
            {
                SetError($"Payment failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CompleteSale(List<PaymentDto> payments)
        {
            try
            {
                var saleDto = new CreateSaleDto
                {
                    BillNumber = BillNumber,
                    CustomerId = SelectedCustomer?.Id,
                    Items = CartItems.Select(x => new SaleItemDto
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        SKU = x.SKU,
                        Unit = x.UnitName,
                        HSNCode = x.HSNCode,
                        Quantity = x.Quantity,
                        UnitPrice = x.ActualPrice,
                        DiscountPercent = x.DiscountPercent ?? 0,
                        DiscountAmount = x.DiscountAmount,
                        TaxRate = x.TaxRate,
                        TaxAmount = x.TaxAmount,
                        TotalAmount = x.TotalAmount,
                        TaxProfileId = x.TaxProfileIdValue
                    }).ToList(),
                    Payments = payments,
                    Subtotal = Subtotal,
                    DiscountAmount = DiscountAmount,
                    TaxAmount = TotalTax,
                    GrandTotal = Math.Round(GrandTotal),
                    LoyaltyPointsRedeemed = PointsToRedeem,
                    LoyaltyRedemptionAmount = LoyaltyRedemptionAmount
                };

                var receipt = await _billingApi.CreateSaleAsync(saleDto);
                
                if (receipt != null)
                {
                    _printSettings?.SaveLastPrintedSaleId(receipt.SaleId);
                    ShowReceiptPreview?.Invoke(receipt);
                    ClearCart();
                    SetSuccess("Sale completed successfully!");
                }
            }
            catch (Exception ex)
            {
                SetError($"Failed to complete sale: {ex.Message}");
            }
        }

        private async Task LoadCustomers()
        {
            try
            {
                var customers = await _customerApi.GetAllAsync();
                Customers = new ObservableCollection<CustomerDto>(customers ?? new List<CustomerDto>());
            }
            catch (Exception ex)
            {
                SetError($"Failed to load customers: {ex.Message}");
            }
        }

        private async Task GenerateBillNumber()
        {
            try
            {
                BillNumber = await _billingApi.GenerateBillNumberAsync();
                SetSuccess("Bill number generated");
            }
            catch (Exception ex)
            {
                SetError($"Failed to generate bill number: {ex.Message}");
            }
        }

        private async Task RedeemPointsAsync()
        {
            if (SelectedCustomer == null)
            {
                SetError("Select a customer before redeeming points.");
                return;
            }

            if (PointsToRedeem <= 0)
            {
                SetError("Enter points to redeem.");
                return;
            }

            try
            {
                IsBusy = true;
                var result = await _loyaltyApi.RedeemPointsAsync(SelectedCustomer.Id, PointsToRedeem);
                if (result == null)
                {
                    SetError("Failed to redeem points.");
                    return;
                }

                PointsToRedeem = result.PointsRedeemed;
                LoyaltyRedemptionAmount = result.RedemptionAmount;
                AvailableLoyaltyPoints = result.RemainingPoints;

                RecalculateTotals();
                SetSuccess($"Redeemed {PointsToRedeem} points for ₹{LoyaltyRedemptionAmount:N2}.");
            }
            catch (Exception ex)
            {
                SetError($"Failed to redeem points: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public string GetCartJson()
        {
            var cartData = new CartDataDto
            {
                Items = CartItems.Select(x => new CartItemDto
                {
                    ProductId = x.ProductId,
                    SKU = x.SKU,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    ActualPrice = x.ActualPrice,
                    DiscountPercent = x.DiscountPercent,
                    TaxRate = x.TaxRate
                }).ToList(),
                CustomerId = SelectedCustomer?.Id,
                DiscountInputValue = DiscountInputValue,
                IsDiscountByPercent = IsDiscountByPercent
            };

            return System.Text.Json.JsonSerializer.Serialize(cartData);
        }

        public void LoadCartFromJson(string json)
        {
            try
            {
                var cartData = System.Text.Json.JsonSerializer.Deserialize<CartDataDto>(json);
                if (cartData == null) return;

                ClearCart();

                foreach (var item in cartData.Items)
                {
                    var cartItem = new CartItem
                    {
                        ProductId = item.ProductId,
                        LineNumber = CartItems.Count + 1,
                        SKU = item.SKU,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        ActualPrice = item.ActualPrice,
                        DiscountPercent = item.DiscountPercent,
                        TaxRate = item.TaxRate
                    };

                    CartItems.Add(cartItem);
                }

                if (cartData.CustomerId.HasValue)
                {
                    SelectedCustomer = Customers.FirstOrDefault(x => x.Id == cartData.CustomerId);
                }

                DiscountInputValue = cartData.DiscountInputValue ?? string.Empty;
                IsDiscountByPercent = cartData.IsDiscountByPercent;

                RecalculateTotals();
            }
            catch (Exception ex)
            {
                SetError($"Failed to load cart: {ex.Message}");
            }
        }

        private void SetSuccess(string message)
        {
            StatusMessage = message;
            IsStatusError = false;
        }

        private void SetError(string message)
        {
            StatusMessage = message;
            IsStatusError = true;
        }
    }
}
