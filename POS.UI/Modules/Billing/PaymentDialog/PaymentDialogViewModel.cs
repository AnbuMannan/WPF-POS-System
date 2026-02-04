using System.Collections.ObjectModel;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;

namespace POS.UI.Modules.Billing.PaymentDialog
{
    public class PaymentDialogViewModel : ViewModelBase
    {
        private decimal _totalAmount;
        private decimal _totalPaid;
        private decimal _balance;
        private string _cashTenderedText = string.Empty;
        private string _splitMethod = "Cash";
        private string _splitAmountText = string.Empty;

        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); CalculateBalance(); RaiseCompleteSaleCanExecute(); _addCashPaymentCommand?.RaiseCanExecuteChanged(); }
        }

        public decimal TotalPaid
        {
            get => _totalPaid;
            set { _totalPaid = value; OnPropertyChanged(); CalculateBalance(); RaiseCompleteSaleCanExecute(); }
        }

        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        /// <summary>Amount of cash received from customer (for change calculation). String for reliable TextBox binding.</summary>
        public string CashTenderedText
        {
            get => _cashTenderedText;
            set { _cashTenderedText = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(ChangeAmount)); OnPropertyChanged(nameof(ChangeAmountText)); OnPropertyChanged(nameof(CanAddCashPayment)); _addCashPaymentCommand?.RaiseCanExecuteChanged(); }
        }

        private decimal CashTenderedDecimal => decimal.TryParse(CashTenderedText?.Trim().Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;

        /// <summary>Change to give back when CashTendered >= TotalAmount.</summary>
        public decimal ChangeAmount => CashTenderedDecimal >= TotalAmount ? CashTenderedDecimal - TotalAmount : 0;
        public string ChangeAmountText => ChangeAmount.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>Enable when any positive amount is entered so user can add full or partial cash.</summary>
        public bool CanAddCashPayment => CashTenderedDecimal > 0;

        private RelayCommand? _addCashPaymentCommand;

        public string SplitMethod
        {
            get => _splitMethod;
            set { _splitMethod = value; OnPropertyChanged(); }
        }

        /// <summary>Split payment amount as string for reliable TextBox binding.</summary>
        public string SplitAmountText
        {
            get => _splitAmountText;
            set { _splitAmountText = value ?? string.Empty; OnPropertyChanged(); }
        }
        private decimal SplitAmountDecimal => decimal.TryParse(SplitAmountText?.Trim().Replace(",", ""), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;

        /// <summary>Options for split payment method dropdown.</summary>
        public static string[] PaymentMethodOptions { get; } = { "Cash", "Card", "UPI" };

        public ObservableCollection<PaymentDto> CompletedPayments { get; } = new();

        public ICommand AddCashPaymentCommand { get; }
        public ICommand AddCardPaymentCommand { get; }
        public ICommand AddUPIPaymentCommand { get; }
        public ICommand AddSplitPaymentCommand { get; }
        public ICommand CompleteSaleCommand { get; }

        public Action? RequestClose { get; set; }

        private RelayCommand? _completeSaleCommand;

        public PaymentDialogViewModel(decimal totalAmount)
        {
            TotalAmount = totalAmount;
            Balance = totalAmount;
            SplitAmountText = totalAmount.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);

            _addCashPaymentCommand = new RelayCommand(AddCashPayment, () => CanAddCashPayment);
            AddCashPaymentCommand = _addCashPaymentCommand;
            AddCardPaymentCommand = new RelayCommand(() => AddPayment("Card", Balance));
            AddUPIPaymentCommand = new RelayCommand(() => AddPayment("UPI", Balance));
            AddSplitPaymentCommand = new RelayCommand(AddSplitPaymentEntry);
            _completeSaleCommand = new RelayCommand(CompleteSale, () => TotalPaid >= TotalAmount && TotalAmount > 0);
            CompleteSaleCommand = _completeSaleCommand;

            CompletedPayments.CollectionChanged += (s, e) =>
            {
                TotalPaid = CompletedPayments.Sum(x => x.Amount);
                RaiseCompleteSaleCanExecute();
            };
        }

        private void AddCashPayment()
        {
            if (!CanAddCashPayment) return;
            decimal amountToAdd = Math.Min(CashTenderedDecimal, Balance > 0 ? Balance : TotalAmount);
            if (amountToAdd <= 0) return;
            AddPayment("Cash", amountToAdd);
            // Clear or reduce tendered: if we paid in full, clear; else show remainder
            if (CashTenderedDecimal >= TotalAmount && amountToAdd >= TotalAmount)
                CashTenderedText = string.Empty;
            else
                CashTenderedText = (CashTenderedDecimal - amountToAdd).ToString("N2", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void AddSplitPaymentEntry()
        {
            decimal amount = SplitAmountDecimal > 0 ? Math.Min(SplitAmountDecimal, Balance) : Balance;
            if (amount <= 0) return;
            AddPayment(SplitMethod, amount);
            SplitAmountText = Balance.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void AddPayment(string method, decimal amount)
        {
            if (amount <= 0) return;
            CompletedPayments.Add(new PaymentDto
            {
                PaymentMethod = method,
                Amount = amount,
                PaymentDate = DateTime.Now
            });
            TotalPaid = CompletedPayments.Sum(x => x.Amount);
        }

        private void CompleteSale()
        {
            if (TotalPaid >= TotalAmount)
                RequestClose?.Invoke();
        }

        private void CalculateBalance()
        {
            Balance = Math.Max(0, TotalAmount - TotalPaid);
        }

        private void RaiseCompleteSaleCanExecute()
        {
            _completeSaleCommand?.RaiseCanExecuteChanged();
        }
    }
}
