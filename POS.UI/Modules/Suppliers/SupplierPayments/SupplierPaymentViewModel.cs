using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.SupplierPayments
{
    public class SupplierPaymentViewModel : ViewModelBase
    {
        private readonly SupplierPaymentApiService _paymentService;
        private readonly SupplierApiService _supplierService;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<SupplierBalanceDto> SupplierBalances { get; set; } = new();
        public ObservableCollection<SupplierPaymentDto> RecentPayments { get; set; } = new();
        public ObservableCollection<SupplierTransactionDto> SupplierLedger { get; set; } = new();

        private List<SupplierBalanceDto> _allSupplierBalances = new();

        // ================= SUPPLIER SELECTION (LEFT PANEL) =================

        private SupplierBalanceDto? _selectedSupplier;
        public SupplierBalanceDto? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSupplierSelected));
                OnPropertyChanged(nameof(SelectedSupplierBalance));
                _ = LoadSupplierDetailsAsync();
                ((RelayCommand)SavePaymentCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsSupplierSelected => SelectedSupplier != null;

        public string SelectedSupplierBalance => SelectedSupplier != null
            ? $"₹ {SelectedSupplier.CurrentBalance:N2}"
            : "₹ 0.00";

        // ================= PAYMENT FORM (RIGHT PANEL) =================

        private DateTime _paymentDate = DateTime.Now;
        public DateTime PaymentDate
        {
            get => _paymentDate;
            set
            {
                _paymentDate = value;
                OnPropertyChanged();
            }
        }

        private decimal _paymentAmount;
        public decimal PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                OnPropertyChanged();
                ((RelayCommand)SavePaymentCommand).RaiseCanExecuteChanged();
            }
        }

        private string _selectedPaymentMode = "Cash";
        public string SelectedPaymentMode
        {
            get => _selectedPaymentMode;
            set
            {
                _selectedPaymentMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowBankFields));
            }
        }

        public List<string> PaymentModes { get; } = new()
        {
            "Cash",
            "Bank Transfer",
            "Cheque",
            "UPI",
            "Credit Card",
            "Other"
        };

        public bool ShowBankFields => SelectedPaymentMode != "Cash";

        private string? _referenceNo;
        public string? ReferenceNo
        {
            get => _referenceNo;
            set
            {
                _referenceNo = value;
                OnPropertyChanged();
            }
        }

        private string? _bankName;
        public string? BankName
        {
            get => _bankName;
            set
            {
                _bankName = value;
                OnPropertyChanged();
            }
        }

        private string? _remarks;
        public string? Remarks
        {
            get => _remarks;
            set
            {
                _remarks = value;
                OnPropertyChanged();
            }
        }

        // ================= SEARCH =================

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }

        // ================= LOADING STATE =================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged();
                ((RelayCommand)SavePaymentCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= COMMANDS =================

        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand SavePaymentCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand PayFullBalanceCommand { get; }

        // ================= CONSTRUCTOR =================

        public SupplierPaymentViewModel(SupplierPaymentApiService paymentService, SupplierApiService supplierService)
        {
            _paymentService = paymentService;
            _supplierService = supplierService;

            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyDisplayFilter();
            };

            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(ApplyDisplayFilter);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            SavePaymentCommand = new RelayCommand(async () => await SavePaymentAsync(), CanSavePayment);
            ClearFormCommand = new RelayCommand(ClearForm);
            PayFullBalanceCommand = new RelayCommand(PayFullBalance, () => SelectedSupplier != null && SelectedSupplier.CurrentBalance > 0);

            _ = LoadAsync();
        }

        // ================= LOAD DATA =================

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var balances = await _paymentService.GetAllBalancesAsync();
                _allSupplierBalances = balances ?? new List<SupplierBalanceDto>();
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Load Failed", $"Failed to load supplier balances: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSupplierDetailsAsync()
        {
            if (SelectedSupplier == null)
            {
                RecentPayments.Clear();
                SupplierLedger.Clear();
                return;
            }

            try
            {
                // Load recent payments for this supplier
                var payments = await _paymentService.GetBySupplierAsync(SelectedSupplier.SupplierId);
                RecentPayments.Clear();
                foreach (var p in payments.OrderByDescending(x => x.PaymentDate).Take(10))
                    RecentPayments.Add(p);

                // Load ledger/transactions
                var ledger = await _paymentService.GetLedgerAsync(SelectedSupplier.SupplierId);
                SupplierLedger.Clear();
                foreach (var t in ledger.OrderByDescending(x => x.TransactionDate).Take(20))
                    SupplierLedger.Add(t);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Load Failed", $"Failed to load supplier details: {ex.Message}");
            }
        }

        // ================= SEARCH & FILTER =================

        private void ApplyDisplayFilter()
        {
            var filtered = _allSupplierBalances.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(x =>
                    x.SupplierName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    x.SupplierCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (x.Mobile?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            SupplierBalances.Clear();
            foreach (var item in filtered.OrderByDescending(x => x.CurrentBalance).ToList())
                SupplierBalances.Add(item);
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            ApplyDisplayFilter();
        }

        // ================= PAYMENT ACTIONS =================

        private bool CanSavePayment()
        {
            return SelectedSupplier != null
                   && PaymentAmount > 0
                   && !IsSaving;
        }

        private async Task SavePaymentAsync()
        {
            if (SelectedSupplier == null || PaymentAmount <= 0)
                return;

            // Confirm payment
            var confirmResult = POS.UI.Components.DialogService.Confirm(
                "Confirm Payment",
                $"Record payment of ₹{PaymentAmount:N2} to {SelectedSupplier.SupplierName}?");

            if (confirmResult != MessageBoxResult.Yes)
                return;

            try
            {
                IsSaving = true;

                var dto = new CreateSupplierPaymentDto
                {
                    SupplierId = SelectedSupplier.SupplierId,
                    PaymentDate = PaymentDate,
                    Amount = PaymentAmount,
                    PaymentMode = SelectedPaymentMode,
                    ReferenceNo = ReferenceNo,
                    BankName = BankName,
                    Remarks = Remarks
                };

                var result = await _paymentService.CreateAsync(dto);

                POS.UI.Components.DialogService.Info(
                    "Payment Saved",
                    $"Payment recorded successfully!\nPayment No: {result.PaymentNo}");

                ClearForm();
                await LoadAsync();

                // Re-select the same supplier to refresh their balance
                var supplierId = SelectedSupplier.SupplierId;
                await LoadSupplierDetailsAsync();

                // Update selection if supplier still exists in list
                SelectedSupplier = SupplierBalances.FirstOrDefault(x => x.SupplierId == supplierId);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Payment Failed", ex.Message);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ClearForm()
        {
            PaymentDate = DateTime.Now;
            PaymentAmount = 0;
            SelectedPaymentMode = "Cash";
            ReferenceNo = null;
            BankName = null;
            Remarks = null;
        }

        private void PayFullBalance()
        {
            if (SelectedSupplier != null && SelectedSupplier.CurrentBalance > 0)
            {
                PaymentAmount = SelectedSupplier.CurrentBalance;
            }
        }

        // ================= KEYBOARD SHORTCUTS =================

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                _ = LoadAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F2 && CanSavePayment())
            {
                _ = SavePaymentAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                ClearForm();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                // Focus search - handled in view
                e.Handled = true;
            }
        }
    }
}
