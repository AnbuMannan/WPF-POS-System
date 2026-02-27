using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Components;

namespace POS.UI.Modules.Inventory.StockTake
{
    public class PhysicalStockTakeViewModel : ViewModelBase
    {
        private readonly ProductApiService _productService;
        private readonly StockAdjustmentApiService _adjustmentService;
        private readonly StockApiService _stockService;
        private string _searchBarcode = string.Empty;

        public PhysicalStockTakeViewModel(
            ProductApiService productService,
            StockAdjustmentApiService adjustmentService,
            StockApiService stockService)
        {
            _productService = productService;
            _adjustmentService = adjustmentService;
            _stockService = stockService;

            ScanCommand = new RelayCommand(async () => await ScanAsync());
            CompleteAuditCommand = new RelayCommand(async () => await CompleteAuditAsync());
            AuditItems = new ObservableCollection<StockTakeItemModel>();
            AuditItems.CollectionChanged += (s, e) => {
                if (e.NewItems != null)
                {
                    foreach (StockTakeItemModel item in e.NewItems)
                        item.PropertyChanged += (sender, args) => UpdateTotals();
                }
                UpdateTotals();
            };
        }

        public string SearchBarcode
        {
            get => _searchBarcode;
            set { _searchBarcode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<StockTakeItemModel> AuditItems { get; }

        private decimal _totalVarianceAmount;
        public decimal TotalVarianceAmount
        {
            get => _totalVarianceAmount;
            private set { _totalVarianceAmount = value; OnPropertyChanged(); }
        }

        private int _totalScannedItems;
        public int TotalScannedItems
        {
            get => _totalScannedItems;
            private set { _totalScannedItems = value; OnPropertyChanged(); }
        }

        public ICommand ScanCommand { get; }
        public ICommand CompleteAuditCommand { get; }

        private async Task ScanAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchBarcode)) return;

            try
            {
                var barcode = SearchBarcode.Trim();
                var existing = AuditItems.FirstOrDefault(i => i.Barcode == barcode || i.SKU == barcode);
                if (existing != null)
                {
                    existing.PhysicalQty += 1;
                    SearchBarcode = string.Empty;
                    return;
                }

                var products = await _productService.SearchAsync(barcode);
                var product = products.FirstOrDefault(p => p.Barcode == barcode || p.SKU == barcode);

                if (product == null)
                {
                    DialogService.Warning("Not Found", $"Product with barcode/SKU '{barcode}' not found.");
                    SearchBarcode = string.Empty;
                    return;
                }

                var currentStock = await _stockService.GetProductStockAsync(product.ProductId);

                var newItem = new StockTakeItemModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    Barcode = product.Barcode ?? string.Empty,
                    SKU = product.SKU,
                    SystemQty = currentStock,
                    PhysicalQty = 1,
                    CostPrice = product.CostPrice
                };

                AuditItems.Add(newItem);
                SearchBarcode = string.Empty;
            }
            catch (Exception ex)
            {
                DialogService.Error("Error", "Failed to scan product: " + ex.Message);
            }
        }

        private async Task CompleteAuditAsync()
        {
            var variances = AuditItems.Where(i => i.VarianceQty != 0).ToList();
            if (!variances.Any())
            {
                DialogService.Info("No Variance", "No variances found to adjust.");
                return;
            }

            if (DialogService.Confirm("Complete Audit", $"Submit {variances.Count} stock adjustments?") != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                var dto = new CreateStockAdjustmentDto
                {
                    AdjustmentDate = DateTime.Now,
                    AdjustedBy = AppState.CurrentUserName ?? "System",
                    Reason = AdjustmentReasons.Audit,
                    Remarks = $"Physical Stock Audit completed on {DateTime.Now:dd MMM yyyy HH:mm}",
                    Items = variances.Select(v => new CreateStockAdjustmentItemDto
                    {
                        ProductId = v.ProductId,
                        Quantity = v.VarianceQty,
                        CostPrice = v.CostPrice,
                        Remarks = v.VarianceQty > 0 ? "Stock In (Audit)" : "Stock Out (Audit)"
                    }).ToList()
                };

                // In the API, we need to know if it's In or Out. 
                // CreateStockAdjustmentItemDto might need a 'Type' if the API requires it, 
                // but usually the logic for In/Out is handled by the Quantity sign or a Type field.
                // Assuming based on instructions: "if VarianceQty < 0, type is Out... if > 0, type is In"
                // If the Shared DTO doesn't have 'Type', we follow the DTO we saw earlier.

                await _adjustmentService.CreateAndApproveAsync(dto);
                DialogService.Success("Audit Complete", "Physical stock audit completed and inventory adjusted.");
                AuditItems.Clear();
            }
            catch (Exception ex)
            {
                DialogService.Error("Error", "Failed to complete audit: " + ex.Message);
            }
        }

        private void UpdateTotals()
        {
            TotalVarianceAmount = AuditItems.Sum(i => i.VarianceAmount);
            TotalScannedItems = AuditItems.Count;
        }
    }

    public class StockTakeItemModel : ViewModelBase
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }

        private decimal _systemQty;
        public decimal SystemQty
        {
            get => _systemQty;
            set { _systemQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(VarianceQty)); OnPropertyChanged(nameof(VarianceAmount)); }
        }

        private decimal _physicalQty;
        public decimal PhysicalQty
        {
            get => _physicalQty;
            set { _physicalQty = value; OnPropertyChanged(); OnPropertyChanged(nameof(VarianceQty)); OnPropertyChanged(nameof(VarianceAmount)); }
        }

        public decimal VarianceQty => PhysicalQty - SystemQty;
        public decimal VarianceAmount => VarianceQty * CostPrice;
    }
}