using System.Collections.ObjectModel;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using DialogService = POS.UI.Components.DialogService;

namespace POS.UI.Modules.Reports.EODReport
{
    /// <summary>Display item for payment method breakdown (for binding in view).</summary>
    public class PaymentBreakdownItem
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AmountFormatted => Amount.ToString("N2");
    }

    public class EODReportViewModel : ViewModelBase
    {
        private readonly EODReportApiService _service;
        private readonly IPrintService? _printService;

        private DateTime _reportDate = DateTime.Today;
        public DateTime ReportDate
        {
            get => _reportDate;
            set { _reportDate = value; OnPropertyChanged(); }
        }

        private EODReportDto? _report;
        public EODReportDto? Report
        {
            get => _report;
            set
            {
                _report = value;
                OnPropertyChanged();
                RefreshDerivedFromReport();
            }
        }

        private decimal _openingCash;
        public decimal OpeningCash
        {
            get => _openingCash;
            set { _openingCash = value; OnPropertyChanged(); RecomputeCash(); }
        }

        private decimal _actualCash;
        public decimal ActualCash
        {
            get => _actualCash;
            set { _actualCash = value; OnPropertyChanged(); RecomputeCash(); }
        }

        public decimal ExpectedCash => _expectedCash;
        public decimal CashDifference => _cashDifference;
        public bool IsCashShortage => _cashDifference < 0;
        public bool IsCashOverage => _cashDifference > 0;

        private decimal _expectedCash;
        private decimal _cashDifference;

        public int TotalSalesCount => Report?.SaleCount ?? 0;
        public decimal TotalRevenue => Report?.TotalSales ?? 0;
        public decimal TotalCGST => Report?.TotalCGST ?? 0;
        public decimal TotalSGST => Report?.TotalSGST ?? 0;
        public decimal TotalIGST => Report?.TotalIGST ?? 0;
        public decimal TotalDiscountsGiven => Report?.DiscountSum ?? 0;
        public int TotalReturnsCount => Report?.TotalReturnsCount ?? 0;
        public decimal TotalRefunds => Report?.TotalRefunds ?? 0;
        public decimal CashSalesAmount => Report?.CashSalesAmount ?? 0;
        public decimal CashRefundAmount => Report?.CashRefundAmount ?? 0;
        public int StoreCode => POS.UI.Core.AppState.CurrentStoreCode;

        public ObservableCollection<PaymentBreakdownItem> PaymentBreakdownList { get; } = new();
        public ObservableCollection<EODSaleSummaryDto> TopSales { get; } = new();
        public ObservableCollection<EODTopProductDto> TopSellingProducts { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string? _message;
        public string? Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand PrintReportCommand { get; }
        public ICommand CloseDayCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public EODReportViewModel(EODReportApiService? service, IPrintService? printService = null)
        {
            _service = service!;
            _printService = printService;
            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync(), () => !IsLoading);
            PrintReportCommand = new RelayCommand(PrintReport, () => Report != null);
            CloseDayCommand = new RelayCommand(async () => await CloseDayAsync(), () => Report != null && !IsLoading);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, () => Report != null);
        }

        private void RefreshDerivedFromReport()
        {
            OnPropertyChanged(nameof(TotalSalesCount));
            OnPropertyChanged(nameof(TotalRevenue));
            OnPropertyChanged(nameof(TotalCGST));
            OnPropertyChanged(nameof(TotalSGST));
            OnPropertyChanged(nameof(TotalIGST));
            OnPropertyChanged(nameof(TotalDiscountsGiven));
            OnPropertyChanged(nameof(TotalReturnsCount));
            OnPropertyChanged(nameof(TotalRefunds));
            OnPropertyChanged(nameof(CashSalesAmount));
            OnPropertyChanged(nameof(CashRefundAmount));

            PaymentBreakdownList.Clear();
            if (Report?.PaymentBreakdown != null)
            {
                foreach (var kv in Report.PaymentBreakdown)
                    PaymentBreakdownList.Add(new PaymentBreakdownItem { Method = kv.Key, Amount = kv.Value });
            }

            TopSales.Clear();
            if (Report?.TopSales != null)
            {
                foreach (var s in Report.TopSales)
                    TopSales.Add(s);
            }

            TopSellingProducts.Clear();
            if (Report?.TopSellingProducts != null)
            {
                foreach (var p in Report.TopSellingProducts)
                    TopSellingProducts.Add(p);
            }

            RecomputeCash();
        }

        private void RecomputeCash()
        {
            decimal cashSales = Report?.CashSalesAmount ?? 0;
            decimal cashRefunds = Report?.CashRefundAmount ?? 0;
            decimal totalExpenses = Report?.TotalExpenses ?? 0;
            _expectedCash = OpeningCash + cashSales - cashRefunds - totalExpenses;
            _cashDifference = ActualCash - _expectedCash;
            OnPropertyChanged(nameof(ExpectedCash));
            OnPropertyChanged(nameof(CashDifference));
            OnPropertyChanged(nameof(IsCashShortage));
            OnPropertyChanged(nameof(IsCashOverage));
        }

        private async Task GenerateReportAsync()
        {
            if (_service == null)
            {
                Message = "Report service not available.";
                return;
            }
            IsLoading = true;
            Message = null;
            Report = null;
            try
            {
                var dto = await _service.GetEODReportAsync(ReportDate);
                Report = dto;
                if (dto != null)
                    Message = $"Report generated for {ReportDate:dd-MMM-yyyy}.";
                else
                    Message = "No data returned.";
            }
            catch (Exception ex)
            {
                Message = "Failed to load report: " + ex.Message;
                DialogService.Error("EOD Report", ex.Message);
            }
            finally
            {
                IsLoading = false;
                RaiseCommandsCanExecuteChanged();
            }
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            if (GenerateReportCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            if (PrintReportCommand is RelayCommand rc2) rc2.RaiseCanExecuteChanged();
            if (CloseDayCommand is RelayCommand rc3) rc3.RaiseCanExecuteChanged();
            if (ExportToExcelCommand is RelayCommand rc4) rc4.RaiseCanExecuteChanged();
        }

        private void PrintReport()
        {
            if (Report == null) return;
            try
            {
                _printService?.PrintEODReport(Report, ReportDate, OpeningCash, ActualCash, ExpectedCash, CashDifference);
                Message = "Print requested.";
            }
            catch (Exception ex)
            {
                DialogService.Error("Print EOD Report", ex.Message);
            }
        }

        private async Task CloseDayAsync()
        {
            if (Report == null) return;
            try
            {
                var confirm = DialogService.Confirm("Close Day", $"Lock all sales for {ReportDate:dd-MMM-yyyy}? This will prevent editing of those transactions.");
                if (confirm != System.Windows.MessageBoxResult.Yes) return;
                await _service.CloseDayAsync(ReportDate);
                Message = "Day closed. Sales for this date are now locked.";
                DialogService.Info("Close Day", Message);
            }
            catch (Exception ex)
            {
                Message = "Close day failed: " + ex.Message;
                DialogService.Error("Close Day", ex.Message);
            }
            finally
            {
                RaiseCommandsCanExecuteChanged();
            }
        }

        private void ExportToExcel()
        {
            if (Report == null) return;
            try
            {
                EODExportService.ExportToCsv(Report, ReportDate, OpeningCash, ActualCash, ExpectedCash, CashDifference);
                Message = "Exported to CSV.";
                DialogService.Info("Export", "Report exported to CSV in your Documents folder (or application directory).");
            }
            catch (Exception ex)
            {
                DialogService.Error("Export", ex.Message);
            }
        }
    }
}
