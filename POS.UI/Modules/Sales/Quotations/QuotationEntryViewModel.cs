using POS.Shared.Models;
using POS.UI.Components;
using POS.UI.Core;
using POS.UI.Core.Services;
using POS.UI.Modules.Sales.Returns;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Sales.Quotations;

public class QuotationEntryViewModel : INotifyPropertyChanged
{
    private readonly QuotationApiService _quotationService;
    private readonly ProductApiService _productService;
    private readonly CustomerApiService _customerService;
    private Guid? _editId;

    // Header
    private string? _customerName;
    public string? CustomerName
    {
        get => _customerName;
        set { _customerName = value; OnPropertyChanged(); }
    }

    private string? _customerPhone;
    public string? CustomerPhone
    {
        get => _customerPhone;
        set { _customerPhone = value; OnPropertyChanged(); }
    }

    private DateTime? _validUntil = DateTime.Now.AddDays(30);
    public DateTime? ValidUntil
    {
        get => _validUntil;
        set { _validUntil = value; OnPropertyChanged(); }
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set { _notes = value; OnPropertyChanged(); }
    }

    // Product Search
    private string _productSearch = string.Empty;
    public string ProductSearch
    {
        get => _productSearch;
        set { _productSearch = value; OnPropertyChanged(); }
    }

    // Items
    public ObservableCollection<QuotationItemRow> Items { get; set; } = new();

    public decimal Subtotal => Items.Sum(i => i.Quantity * i.UnitPrice);
    public decimal DiscountTotal => Items.Sum(i => i.DiscountAmount);
    public decimal TaxTotal => Items.Sum(i => i.TaxAmount);
    public decimal GrandTotal => Items.Sum(i => i.TotalAmount);

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand SearchProductCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? QuotationSaved;
    public event Action? RequestClose;

    public QuotationEntryViewModel(QuotationApiService quotationService, ProductApiService productService, CustomerApiService customerService)
    {
        _quotationService = quotationService;
        _productService = productService;
        _customerService = customerService;

        SearchProductCommand = new RelayCommand(async _ => await SearchProduct());
        RemoveItemCommand = new RelayCommand(p =>
        {
            if (p is QuotationItemRow row) Items.Remove(row);
            RecalcTotals();
        });
        SaveCommand = new RelayCommand(async _ => await SaveQuotation(), _ => Items.Count > 0);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
    }

    public async Task LoadForEdit(Guid quotationId)
    {
        _editId = quotationId;
        var q = await _quotationService.GetByIdAsync(quotationId);
        if (q == null) return;

        CustomerName = q.CustomerName;
        CustomerPhone = q.CustomerPhone;
        ValidUntil = q.ValidUntil;
        Notes = q.Notes;

        Items.Clear();
        foreach (var item in q.Items)
        {
            var row = new QuotationItemRow
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                SKU = item.SKU,
                HSNCode = item.HSNCode,
                Quantity = item.Quantity,
                UnitName = item.UnitName,
                UnitPrice = item.UnitPrice,
                DiscountPercent = item.DiscountPercent,
                DiscountAmount = item.DiscountAmount,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount
            };
            row.PropertyChanged += (_, _) => RecalcTotals();
            Items.Add(row);
        }
        RecalcTotals();
    }

    private async Task SearchProduct()
    {
        if (string.IsNullOrWhiteSpace(ProductSearch)) return;

        try
        {
            var products = await _productService.SearchAsync(ProductSearch.Trim());
            if (products == null || products.Count == 0)
            {
                DialogService.Warning("Product Search", "No products found.");
                return;
            }

            // Add first match (simplified - in production, show a popup)
            var product = products.First();
            var row = new QuotationItemRow
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                SKU = product.SKU,
                HSNCode = product.HSNCode,
                Quantity = 1,
                UnitName = product.Unit ?? "PCS",
                UnitPrice = product.SellingPrice,
                TaxRate = 0, // Tax rate will be computed from tax profile if needed
                TaxAmount = 0,
                TotalAmount = product.SellingPrice
            };
            row.PropertyChanged += (_, _) => RecalcTotals();
            Items.Add(row);
            ProductSearch = string.Empty;
            RecalcTotals();
        }
        catch (Exception ex)
        {
            DialogService.Error("Product Search", $"Error: {ex.Message}");
        }
    }

    private async Task SaveQuotation()
    {
        if (Items.Count == 0)
        {
            DialogService.Warning("Save Quotation", "Please add at least one item.");
            return;
        }

        IsLoading = true;
        try
        {
            var dto = new CreateQuotationDto
            {
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone,
                ValidUntil = ValidUntil,
                Notes = Notes,
                Items = Items.Select(i => new CreateQuotationItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    SKU = i.SKU,
                    HSNCode = i.HSNCode,
                    Quantity = i.Quantity,
                    UnitName = i.UnitName,
                    UnitPrice = i.UnitPrice,
                    DiscountPercent = i.DiscountPercent,
                    DiscountAmount = i.DiscountAmount,
                    TaxRate = i.TaxRate,
                    TaxAmount = i.TaxAmount,
                    TotalAmount = i.TotalAmount
                }).ToList()
            };

            if (_editId.HasValue)
            {
                await _quotationService.UpdateAsync(_editId.Value, dto);
                DialogService.Success("Quotation", "Quotation updated successfully.");
            }
            else
            {
                var result = await _quotationService.CreateAsync(dto);
                DialogService.Success("Quotation", $"Quotation {result.QuotationNumber} created successfully.");
            }

            QuotationSaved?.Invoke();
        }
        catch (Exception ex)
        {
            DialogService.Error("Save Quotation", $"Failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void RecalcTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DiscountTotal));
        OnPropertyChanged(nameof(TaxTotal));
        OnPropertyChanged(nameof(GrandTotal));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class QuotationItemRow : INotifyPropertyChanged
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? HSNCode { get; set; }
    public string UnitName { get; set; } = "PCS";

    private decimal _quantity = 1;
    public decimal Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); Recalculate(); }
    }

    private decimal _unitPrice;
    public decimal UnitPrice
    {
        get => _unitPrice;
        set { _unitPrice = value; OnPropertyChanged(); Recalculate(); }
    }

    private decimal _discountPercent;
    public decimal DiscountPercent
    {
        get => _discountPercent;
        set { _discountPercent = value; OnPropertyChanged(); Recalculate(); }
    }

    private decimal _discountAmount;
    public decimal DiscountAmount
    {
        get => _discountAmount;
        set { _discountAmount = value; OnPropertyChanged(); }
    }

    private decimal _taxRate;
    public decimal TaxRate
    {
        get => _taxRate;
        set { _taxRate = value; OnPropertyChanged(); Recalculate(); }
    }

    private decimal _taxAmount;
    public decimal TaxAmount
    {
        get => _taxAmount;
        set { _taxAmount = value; OnPropertyChanged(); }
    }

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set { _totalAmount = value; OnPropertyChanged(); }
    }

    private void Recalculate()
    {
        var lineTotal = Quantity * UnitPrice;
        DiscountAmount = lineTotal * DiscountPercent / 100;
        var taxable = lineTotal - DiscountAmount;
        TaxAmount = taxable * TaxRate / 100;
        TotalAmount = taxable + TaxAmount;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
