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

namespace POS.UI.Modules.Customers.Outstanding;

public class CustomerOutstandingViewModel : INotifyPropertyChanged
{
    private readonly CustomerPaymentApiService _service;

    public ObservableCollection<CustomerBalanceDto> Customers { get; set; } = new();
    public ObservableCollection<CustomerBalanceDto> FilteredCustomers { get; set; } = new();
    public ObservableCollection<CustomerTransactionDto> Transactions { get; set; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
    }

    private CustomerBalanceDto? _selectedCustomer;
    public CustomerBalanceDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set { _selectedCustomer = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCustomerSelected)); _ = LoadTransactionsAsync(); }
    }

    public bool IsCustomerSelected => SelectedCustomer != null;

    private decimal _paymentAmount;
    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set { _paymentAmount = value; OnPropertyChanged(); }
    }

    private string _paymentMode = "Cash";
    public string PaymentMode
    {
        get => _paymentMode;
        set { _paymentMode = value; OnPropertyChanged(); }
    }

    private string _paymentRemarks = string.Empty;
    public string PaymentRemarks
    {
        get => _paymentRemarks;
        set { _paymentRemarks = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> PaymentModes { get; } = new() { "Cash", "Card", "UPI", "BankTransfer", "Cheque" };

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand ReceivePaymentCommand { get; }

    public CustomerOutstandingViewModel(CustomerPaymentApiService service)
    {
        _service = service;
        RefreshCommand = new Sales.Returns.RelayCommand(async _ => await LoadCustomersAsync());
        ReceivePaymentCommand = new Sales.Returns.RelayCommand(async _ => await ReceivePayment(), _ => SelectedCustomer != null && PaymentAmount > 0);

        _ = LoadCustomersAsync();
    }

    public async Task LoadCustomersAsync()
    {
        IsLoading = true;
        try
        {
            var customers = await _service.GetOutstandingCustomersAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Customers.Clear();
                foreach (var c in customers) Customers.Add(c);
                ApplyFilter();
            });
        }
        catch (Exception ex)
        {
            DialogService.Error("Customer Outstanding", $"Failed to load: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTransactionsAsync()
    {
        if (SelectedCustomer == null)
        {
            Transactions.Clear();
            return;
        }

        try
        {
            var ledger = await _service.GetLedgerAsync(SelectedCustomer.CustomerId);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Transactions.Clear();
                if (ledger?.Entries != null)
                {
                    foreach (var entry in ledger.Entries)
                        Transactions.Add(entry);
                }
            });
        }
        catch (Exception ex)
        {
            DialogService.Error("Customer Ledger", $"Failed to load transactions: {ex.Message}");
        }
    }

    private async Task ReceivePayment()
    {
        if (SelectedCustomer == null || PaymentAmount <= 0) return;

        var confirm = DialogService.Confirm("Receive Payment",
            $"Receive {AppState.CurrencySymbol}{PaymentAmount:N2} from {SelectedCustomer.CustomerName}?\n\nMode: {PaymentMode}");

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        try
        {
            var dto = new CustomerPaymentRequestDto
            {
                CustomerId = SelectedCustomer.CustomerId,
                Amount = PaymentAmount,
                PaymentMode = PaymentMode,
                Remarks = PaymentRemarks
            };

            await _service.PayDueAsync(dto);

            DialogService.Success("Payment Received", $"Payment of {AppState.CurrencySymbol}{PaymentAmount:N2} recorded successfully.");
            PaymentAmount = 0;
            PaymentRemarks = string.Empty;

            await LoadCustomersAsync();
            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            DialogService.Error("Payment", $"Failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredCustomers.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Customers
            : new ObservableCollection<CustomerBalanceDto>(
                Customers.Where(c =>
                    (c.CustomerName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)));

        foreach (var c in filtered)
            FilteredCustomers.Add(c);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
