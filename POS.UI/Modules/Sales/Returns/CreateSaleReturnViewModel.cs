using POS.Shared.Models;
using POS.UI.Components;
using POS.UI.Core;
using POS.UI.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Sales.Returns;

public class CreateSaleReturnViewModel : INotifyPropertyChanged
{
    private readonly SaleReturnApiService _service;

    // Step 1: Find Invoice
    private string _invoiceSearch = string.Empty;
    public string InvoiceSearch
    {
        get => _invoiceSearch;
        set { _invoiceSearch = value; OnPropertyChanged(); }
    }

    private SaleInvoiceForReturnDto? _invoice;
    public SaleInvoiceForReturnDto? Invoice
    {
        get => _invoice;
        set { _invoice = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsInvoiceLoaded)); OnPropertyChanged(nameof(InvoiceInfo)); }
    }

    public bool IsInvoiceLoaded => Invoice != null;
    public string InvoiceInfo => Invoice != null
        ? $"Invoice: {Invoice.BillNumber} | Date: {Invoice.CreatedAt:dd MMM yyyy} | Customer: {Invoice.CustomerName ?? "Walk-in"} | Total: {Invoice.TotalAmount:N2}"
        : string.Empty;

    // Step 2: Items
    public ObservableCollection<ReturnItemRow> ReturnItems { get; set; } = new();

    // Step 3: Return details
    private string _reason = string.Empty;
    public string Reason
    {
        get => _reason;
        set { _reason = value; OnPropertyChanged(); }
    }

    private string _selectedRefundMode = "Cash";
    public string SelectedRefundMode
    {
        get => _selectedRefundMode;
        set { _selectedRefundMode = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> RefundModes { get; } = new() { "Cash", "CreditNote", "Card" };

    public decimal TotalRefundAmount => ReturnItems.Where(i => i.IsSelected).Sum(i => i.ReturnAmount);

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand SearchInvoiceCommand { get; }
    public ICommand ProcessReturnCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectAllCommand { get; }

    public event Action? ReturnSaved;
    public event Action? RequestClose;

    public CreateSaleReturnViewModel(SaleReturnApiService service)
    {
        _service = service;
        SearchInvoiceCommand = new RelayCommand(async _ => await SearchInvoice());
        ProcessReturnCommand = new RelayCommand(async _ => await ProcessReturn(), _ => IsInvoiceLoaded && ReturnItems.Any(i => i.IsSelected && i.ReturnQuantity > 0));
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        SelectAllCommand = new RelayCommand(_ => ToggleSelectAll());
    }

    private async Task SearchInvoice()
    {
        if (string.IsNullOrWhiteSpace(InvoiceSearch)) return;

        IsLoading = true;
        try
        {
            var invoice = await _service.LookupInvoiceAsync(InvoiceSearch.Trim());
            if (invoice == null)
            {
                DialogService.Warning("Find Invoice", "Invoice not found. Please check the number and try again.");
                return;
            }

            Invoice = invoice;
            LoadInvoiceItems();
        }
        catch (Exception ex)
        {
            DialogService.Error("Find Invoice", $"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadInvoiceItems()
    {
        ReturnItems.Clear();
        if (Invoice == null) return;

        foreach (var item in Invoice.Items.Where(i => i.MaxReturnQuantity > 0))
        {
            var row = new ReturnItemRow
            {
                SaleItemId = item.SaleItemId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                SKU = item.SKU,
                OriginalQuantity = item.Quantity,
                AlreadyReturned = item.AlreadyReturned,
                MaxReturnQuantity = item.MaxReturnQuantity,
                SellingPrice = item.SellingPrice,
                ReturnQuantity = 0,
                RefundPrice = item.SellingPrice,
                IsRestockable = true,
                IsSelected = false
            };
            row.PropertyChanged += (_, _) => OnPropertyChanged(nameof(TotalRefundAmount));
            ReturnItems.Add(row);
        }
    }

    private void ToggleSelectAll()
    {
        var allSelected = ReturnItems.All(i => i.IsSelected);
        foreach (var item in ReturnItems)
        {
            item.IsSelected = !allSelected;
            if (item.IsSelected && item.ReturnQuantity == 0)
                item.ReturnQuantity = item.MaxReturnQuantity;
        }
        OnPropertyChanged(nameof(TotalRefundAmount));
    }

    private async Task ProcessReturn()
    {
        var selectedItems = ReturnItems.Where(i => i.IsSelected && i.ReturnQuantity > 0).ToList();
        if (!selectedItems.Any())
        {
            DialogService.Warning("Process Return", "Please select at least one item and enter return quantity.");
            return;
        }

        // Validate quantities
        foreach (var item in selectedItems)
        {
            if (item.ReturnQuantity > item.MaxReturnQuantity)
            {
                DialogService.Warning("Process Return", $"Return quantity for {item.ProductName} exceeds maximum ({item.MaxReturnQuantity}).");
                return;
            }
        }

        var confirm = DialogService.Confirm("Process Return",
            $"Create and process return for {selectedItems.Count} item(s)?\n\n" +
            $"Total Refund: {AppState.CurrencySymbol}{TotalRefundAmount:N2}\n" +
            $"Refund Mode: {SelectedRefundMode}");

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            var dto = new CreateSaleReturnDto
            {
                OriginalSaleId = Invoice!.SaleId,
                Reason = Reason,
                RefundMode = SelectedRefundMode,
                Items = selectedItems.Select(i => new CreateSaleReturnItemDto
                {
                    SaleItemId = i.SaleItemId,
                    ProductId = i.ProductId,
                    QuantityReturned = i.ReturnQuantity,
                    RefundPrice = i.RefundPrice,
                    IsRestockable = i.IsRestockable,
                    Reason = i.Reason
                }).ToList()
            };

            var created = await _service.CreateAsync(dto);

            // Auto-process
            await _service.ProcessAsync(created.ReturnId);

            DialogService.Success("Sales Return", $"Return {created.ReturnNumber} created and processed successfully.\nRefund Amount: {AppState.CurrencySymbol}{created.RefundAmount:N2}");
            ReturnSaved?.Invoke();
        }
        catch (Exception ex)
        {
            DialogService.Error("Sales Return", $"Failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ReturnItemRow : INotifyPropertyChanged
{
    public long SaleItemId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal OriginalQuantity { get; set; }
    public decimal AlreadyReturned { get; set; }
    public decimal MaxReturnQuantity { get; set; }
    public decimal SellingPrice { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReturnAmount)); }
    }

    private decimal _returnQuantity;
    public decimal ReturnQuantity
    {
        get => _returnQuantity;
        set
        {
            _returnQuantity = Math.Min(value, MaxReturnQuantity);
            OnPropertyChanged();
            OnPropertyChanged(nameof(ReturnAmount));
        }
    }

    private decimal _refundPrice;
    public decimal RefundPrice
    {
        get => _refundPrice;
        set { _refundPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(ReturnAmount)); }
    }

    private bool _isRestockable = true;
    public bool IsRestockable
    {
        get => _isRestockable;
        set { _isRestockable = value; OnPropertyChanged(); }
    }

    private string? _reason;
    public string? Reason
    {
        get => _reason;
        set { _reason = value; OnPropertyChanged(); }
    }

    public decimal ReturnAmount => IsSelected ? ReturnQuantity * RefundPrice : 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
