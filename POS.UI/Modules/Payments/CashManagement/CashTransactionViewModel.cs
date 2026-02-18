using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Payments.CashManagement
{
    public class CashTransactionViewModel : ViewModelBase
    {
        private readonly CashTransactionApiService _service;

        // Collections
        public ObservableCollection<CashTransactionDto> Transactions { get; set; } = new();

        // Summary
        private decimal _totalCashIn;
        public decimal TotalCashIn
        {
            get => _totalCashIn;
            set { _totalCashIn = value; OnPropertyChanged(); }
        }

        private decimal _totalCashOut;
        public decimal TotalCashOut
        {
            get => _totalCashOut;
            set { _totalCashOut = value; OnPropertyChanged(); }
        }

        private decimal _currentBalance;
        public decimal CurrentBalance
        {
            get => _currentBalance;
            set { _currentBalance = value; OnPropertyChanged(); }
        }

        private int _transactionCount;
        public int TransactionCount
        {
            get => _transactionCount;
            set { _transactionCount = value; OnPropertyChanged(); }
        }

        // Filter
        private DateTime _fromDate = DateTime.Today;
        public DateTime FromDate
        {
            get => _fromDate;
            set { _fromDate = value; OnPropertyChanged(); }
        }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get => _toDate;
            set { _toDate = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadTodayCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CashInCommand { get; }
        public ICommand CashOutCommand { get; }

        public CashTransactionViewModel(CashTransactionApiService service)
        {
            _service = service;

            LoadCommand = new RelayCommand(async () => await LoadTodayAsync());
            RefreshCommand = new RelayCommand(async () => await LoadTodayAsync());
            LoadTodayCommand = new RelayCommand(async () => await LoadTodayAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            CashInCommand = new RelayCommand(async () => await ShowCashInDialogAsync());
            CashOutCommand = new RelayCommand(async () => await ShowCashOutDialogAsync());

            _ = LoadTodayAsync();
        }

        private async Task LoadTodayAsync()
        {
            try
            {
                IsLoading = true;

                var transactions = await _service.GetTodayAsync();
                Transactions.Clear();
                foreach (var tx in transactions)
                    Transactions.Add(tx);

                var summary = await _service.GetTodaySummaryAsync();
                if (summary != null)
                {
                    TotalCashIn = summary.TotalCashIn;
                    TotalCashOut = summary.TotalCashOut;
                    CurrentBalance = summary.CurrentBalance;
                    TransactionCount = summary.TransactionCount;
                }
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to load transactions", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;

                var transactions = await _service.GetAllAsync(FromDate, ToDate);
                Transactions.Clear();
                foreach (var tx in transactions)
                    Transactions.Add(tx);

                var summary = await _service.GetSummaryAsync(FromDate, ToDate);
                if (summary != null)
                {
                    TotalCashIn = summary.TotalCashIn;
                    TotalCashOut = summary.TotalCashOut;
                    CurrentBalance = summary.CurrentBalance;
                    TransactionCount = summary.TransactionCount;
                }
            }
            catch (Exception ex)
            {
                Components.DialogService.Error("Failed to search transactions", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ShowCashInDialogAsync()
        {
            var dialog = new CashEntryDialog("Cash In", true);
            dialog.Owner = System.Windows.Application.Current?.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                var dto = new CreateCashTransactionDto
                {
                    Type = "CashIn",
                    Amount = dialog.Amount,
                    Description = dialog.Description,
                    Category = dialog.Category,
                    Remarks = dialog.Remarks
                };

                var (success, message, _) = await _service.CashInAsync(dto);
                if (success)
                {
                    Components.DialogService.Info("Success", message);
                    await LoadTodayAsync();
                }
                else
                {
                    Components.DialogService.Error("Failed", message);
                }
            }
        }

        private async Task ShowCashOutDialogAsync()
        {
            var dialog = new CashEntryDialog("Cash Out / Expense", false);
            dialog.Owner = System.Windows.Application.Current?.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                var dto = new CreateCashTransactionDto
                {
                    Type = "CashOut",
                    Amount = dialog.Amount,
                    Description = dialog.Description,
                    Category = dialog.Category,
                    Remarks = dialog.Remarks
                };

                var (success, message, _) = await _service.CashOutAsync(dto);
                if (success)
                {
                    Components.DialogService.Info("Success", message);
                    await LoadTodayAsync();
                }
                else
                {
                    Components.DialogService.Error("Failed", message);
                }
            }
        }
    }
}
