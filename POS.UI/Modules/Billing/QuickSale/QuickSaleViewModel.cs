using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Models;
using POS.UI.Components;

namespace POS.UI.Modules.Billing.QuickSale
{
    public class QuickSaleViewModel : ViewModelBase
    {
        private readonly ProductApiService _productService;
        private readonly CategoryApiService _categoryService;
        private readonly BillingApiService _billingService;
        private readonly StockApiService _stockService;
        private readonly IPrintSettingsService? _printSettings;

        private ObservableCollection<CategoryDto> _categories = new();
        private ObservableCollection<ProductDto> _products = new();
        private ObservableCollection<ProductDto> _filteredProducts = new();
        private ObservableCollection<CartItem> _cartItems = new();
        private CartItem? _selectedCartItem;
        private CategoryDto? _selectedCategory;
        private string _searchBarcode = string.Empty;
        private decimal _subtotal;
        private decimal _taxTotal;
        private decimal _grandTotal;

        public QuickSaleViewModel(
            ProductApiService productService,
            CategoryApiService categoryService,
            BillingApiService billingService,
            StockApiService stockService,
            IPrintSettingsService? printSettings = null)
        {
            _productService = productService;
            _categoryService = categoryService;
            _billingService = billingService;
            _stockService = stockService;
            _printSettings = printSettings;

            SelectCategoryCommand = new RelayCommand<CategoryDto>(SelectCategory);
            AddProductCommand = new RelayCommand<ProductDto>(AddProduct);
            ScanBarcodeCommand = new RelayCommand(async () => await ScanBarcodeAsync());
            RemoveItemCommand = new RelayCommand<CartItem>(RemoveItem);
            ClearCartCommand = new RelayCommand(ClearCart);
            QuickPayCommand = new RelayCommand<string>(async (pm) => await QuickPayAsync(pm));

            CartItems.CollectionChanged += (s, e) => RecalculateTotals();
        }

        public ObservableCollection<CategoryDto> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductDto> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProductDto> FilteredProducts
        {
            get => _filteredProducts;
            set { _filteredProducts = value; OnPropertyChanged(); }
        }

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

        public CategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }

        public string SearchBarcode
        {
            get => _searchBarcode;
            set { _searchBarcode = value; OnPropertyChanged(); }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); }
        }

        public decimal TaxTotal
        {
            get => _taxTotal;
            set { _taxTotal = value; OnPropertyChanged(); }
        }

        public decimal GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        public ICommand SelectCategoryCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand QuickPayCommand { get; }

        public async Task InitializeAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                Categories = new ObservableCollection<CategoryDto>(categories);

                var products = await _productService.GetAllAsync();
                foreach (var p in products)
                {
                    p.AvailableStock = await _stockService.GetProductStockAsync(p.ProductId);
                }
                Products = new ObservableCollection<ProductDto>(products);
                
                // Set default category or just show all
                SelectedCategory = Categories.FirstOrDefault();
                if (SelectedCategory == null) FilterProducts(); 
            }
            catch (Exception ex)
            {
                DialogService.Error("Initialization Error", "Failed to load catalog: " + ex.Message);
            }
        }

        private void SelectCategory(CategoryDto? category)
        {
            SelectedCategory = category;
        }

        private void FilterProducts()
        {
            if (SelectedCategory == null)
            {
                FilteredProducts = new ObservableCollection<ProductDto>(Products);
            }
            else
            {
                FilteredProducts = new ObservableCollection<ProductDto>(
                    Products.Where(p => p.CategoryId == SelectedCategory.CategoryId));
            }
        }

        private void AddProduct(ProductDto? product)
        {
            if (product == null) return;

            var productGuid = LongToGuid(product.ProductId);
            var existing = CartItems.FirstOrDefault(i => i.ProductId == productGuid);

            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductId = productGuid,
                    LineNumber = CartItems.Count + 1,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    ActualPrice = product.SellingPrice,
                    MRP = product.MRP,
                    Quantity = 1,
                    UnitName = product.Unit ?? "PCS",
                    HSNCode = product.HSNCode ?? string.Empty,
                    TaxRate = 18m, // Default or fetch from profile
                    TaxProfileIdValue = product.TaxProfileId
                };
                newItem.PropertyChanged += (s, e) => RecalculateTotals();
                CartItems.Add(newItem);
                newItem.RefreshTotals();
            }
            RecalculateTotals();
            RequestFocusToSearch?.Invoke();
        }

        private async Task ScanBarcodeAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchBarcode)) return;

            var barcode = SearchBarcode.Trim();
            var product = Products.FirstOrDefault(p => p.Barcode == barcode || p.SKU == barcode);

            if (product != null)
            {
                AddProduct(product);
            }
            else
            {
                try
                {
                    var p = await _productService.GetByBarcodeAsync(barcode);
                    if (p != null)
                    {
                        p.AvailableStock = await _stockService.GetProductStockAsync(p.ProductId);
                        Products.Add(p);
                        AddProduct(p);
                    }
                }
                catch
                {
                    DialogService.Warning("Not Found", $"Product with barcode/SKU '{barcode}' not found.");
                }
            }

            SearchBarcode = string.Empty;
            RequestFocusToSearch?.Invoke();
        }

        private void RemoveItem(CartItem? item)
        {
            if (item != null)
            {
                CartItems.Remove(item);
                RenumberItems();
            }
        }

        private void RenumberItems()
        {
            for (int i = 0; i < CartItems.Count; i++)
            {
                CartItems[i].LineNumber = i + 1;
            }
        }

        private void ClearCart()
        {
            CartItems.Clear();
            RequestFocusToSearch?.Invoke();
        }

        private void RecalculateTotals()
        {
            Subtotal = CartItems.Sum(i => i.Quantity * i.ActualPrice);
            TaxTotal = CartItems.Sum(i => i.TaxAmount);
            GrandTotal = CartItems.Sum(i => i.TotalAmount);
        }

        private async Task QuickPayAsync(string paymentMethod)
        {
            if (!CartItems.Any())
            {
                DialogService.Warning("Cart Empty", "Please add items to the cart before payment.");
                return;
            }

            try
            {
                var billNumber = await _billingService.GenerateBillNumberAsync();
                var dto = new CreateSaleDto
                {
                    BillNumber = billNumber,
                    CustomerId = null, // Walk-in
                    Subtotal = Subtotal,
                    TaxAmount = TaxTotal,
                    GrandTotal = Math.Round(GrandTotal),
                    Items = CartItems.Select(x => new SaleItemDto
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        SKU = x.SKU,
                        Unit = x.UnitName,
                        Quantity = x.Quantity,
                        UnitPrice = x.ActualPrice,
                        TaxRate = x.TaxRate,
                        TaxAmount = x.TaxAmount,
                        TotalAmount = x.TotalAmount,
                        HSNCode = x.HSNCode,
                        TaxProfileId = x.TaxProfileIdValue
                    }).ToList(),
                    Payments = new List<PaymentDto>
                    {
                        new PaymentDto
                        {
                            PaymentMethod = paymentMethod,
                            Amount = Math.Round(GrandTotal)
                        }
                    }
                };

                var receipt = await _billingService.CreateSaleAsync(dto);
                if (receipt != null)
                {
                    _printSettings?.SaveLastPrintedSaleId(receipt.SaleId);
                    ShowReceiptPreview?.Invoke(receipt);
                    DialogService.Success("Sale Complete", $"Sale processed successfully via {paymentMethod}. Bill: {billNumber}");
                    ClearCart();
                }
            }
            catch (Exception ex)
            {
                DialogService.Error("Payment Error", "Failed to process sale: " + ex.Message);
            }
        }

        private static Guid LongToGuid(long value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public Action? RequestFocusToSearch { get; set; }
        public Action<ReceiptDto>? ShowReceiptPreview { get; set; }
    }
}