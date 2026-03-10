using POS.UI.Core.Services;
using POS.UI.Core.MVVM; // Corrected namespace
using POS.Shared.Models; // Corrected namespace
using POS.UI.Components; // Added for DialogService
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq; // For LINQ operations
using System.Collections.Generic; // For Dictionary

namespace POS.UI.Modules.Reports.EODReport
{
    public class EODReportViewModel : ViewModelBase
    {
        private readonly EODReportApiService _service;
        private readonly IPrintService _printService;
        private readonly PdfExportService _pdfExportService; // Injected for PDF export

        private DateTime _reportDate = DateTime.Today;
        private EODReportDto? _report;
        private bool _isLoading;
        private string _message = string.Empty;

        // Cash Reconciliation properties
        private decimal _openingCash;
        private decimal _actualCash;
        private decimal _expectedCash;
        private decimal _cashDifference;

        // Observable collections for UI binding
        private ObservableCollection<PaymentBreakdownItem> _paymentBreakdownList = new();
        private ObservableCollection<EODSaleSummaryDto> _topSales = new();
        private ObservableCollection<EODTopProductDto> _topSellingProducts = new();

        public EODReportViewModel(EODReportApiService service, IPrintService printService, PdfExportService pdfExportService)
        {
            _service = service;
            _printService = printService;
            _pdfExportService = pdfExportService;

            GenerateReportCommand = new RelayCommand(async () => await GenerateReport(), () => !IsLoading);
            PrintReportCommand = new RelayCommand(PrintReport, () => Report != null);
            PrintToPdfCommand = new RelayCommand(PrintToPdf, () => Report != null); // New command for PDF
            CloseDayCommand = new RelayCommand(async () => await CloseDay(), () => Report != null && !IsLoading);
            ExportToExcelCommand = new RelayCommand(ExportToExcel, () => Report != null);

            // Initial report generation
            // Task.Run(GenerateReport); // This might cause issues with UI thread access, better to call it explicitly or on load.
        }

        public DateTime ReportDate
        {
            get => _reportDate;
            set
            {
                _reportDate = value;
                OnPropertyChanged(nameof(ReportDate));
                (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public EODReportDto? Report
        {
            get => _report;
            set
            {
                _report = value;
                OnPropertyChanged(nameof(Report));
                OnPropertyChanged(nameof(TotalSalesCount));
                OnPropertyChanged(nameof(TotalRevenue));
                OnPropertyChanged(nameof(TotalDiscountsGiven));
                
                // Update PaymentBreakdownList
                PaymentBreakdownList.Clear();
                if (value?.PaymentBreakdown != null)
                {
                    foreach (var item in value.PaymentBreakdown)
                    {
                        PaymentBreakdownList.Add(new PaymentBreakdownItem { Method = item.Key, Amount = item.Value });
                    }
                }

                // Update TopSales
                TopSales.Clear();
                if (value?.TopSales != null)
                {
                    foreach (var item in value.TopSales)
                    {
                        TopSales.Add(item);
                    }
                }

                // Update TopSellingProducts
                TopSellingProducts.Clear();
                if (value?.TopSellingProducts != null)
                {
                    foreach (var item in value.TopSellingProducts)
                    {
                        TopSellingProducts.Add(item);
                    }
                }

                OnPropertyChanged(nameof(TotalCGST));
                OnPropertyChanged(nameof(TotalSGST));
                OnPropertyChanged(nameof(TotalIGST));
                OnPropertyChanged(nameof(TotalReturnsCount));
                OnPropertyChanged(nameof(TotalRefunds));
                OnPropertyChanged(nameof(CashSalesAmount));
                OnPropertyChanged(nameof(CashRefundAmount));
                OnPropertyChanged(nameof(TotalExpenses));
                OnPropertyChanged(nameof(ExpectedCash));
                OnPropertyChanged(nameof(IsCashShortage));
                OnPropertyChanged(nameof(IsCashOverage));
                OnPropertyChanged(nameof(CashDifference));
                

                (PrintReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (PrintToPdfCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CloseDayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ExportToExcelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CloseDayCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        // Properties for Cash Reconciliation
        public decimal OpeningCash
        {
            get => _openingCash;
            set
            {
                _openingCash = value;
                OnPropertyChanged(nameof(OpeningCash));
                CalculateCashDifference();
            }
        }

        public decimal ActualCash
        {
            get => _actualCash;
            set
            {
                _actualCash = value;
                OnPropertyChanged(nameof(ActualCash));
                CalculateCashDifference();
            }
        }

        public decimal ExpectedCash
        {
            get => _expectedCash;
            set
            {
                _expectedCash = value;
                OnPropertyChanged(nameof(ExpectedCash));
                CalculateCashDifference();
            }
        }

        public decimal CashDifference
        {
            get => _cashDifference;
            set
            {
                _cashDifference = value;
                OnPropertyChanged(nameof(CashDifference));
            }
        }

        public bool IsCashShortage => CashDifference < 0;
        public bool IsCashOverage => CashDifference > 0;

        // Report Summary Properties (derived from Report DTO)
        public int TotalSalesCount => Report?.SaleCount ?? 0; // Corrected to SaleCount
        public decimal TotalRevenue => Report?.TotalRevenue ?? 0;
        public decimal TotalDiscountsGiven => Report?.TotalDiscountsGiven ?? 0;
        public ObservableCollection<PaymentBreakdownItem> PaymentBreakdownList
        {
            get => _paymentBreakdownList;
            set
            {
                _paymentBreakdownList = value;
                OnPropertyChanged(nameof(PaymentBreakdownList));
            }
        }
        public decimal TotalCGST => Report?.TotalCGST ?? 0;
        public decimal TotalSGST => Report?.TotalSGST ?? 0;
        public decimal TotalIGST => Report?.TotalIGST ?? 0;
        public int TotalReturnsCount => Report?.TotalReturnsCount ?? 0;
        public decimal TotalRefunds => Report?.TotalRefunds ?? 0;
        public decimal CashSalesAmount => Report?.CashSalesAmount ?? 0;
        public decimal CashRefundAmount => Report?.CashRefundAmount ?? 0;
        public decimal TotalExpenses => Report?.TotalExpenses ?? 0; // This was the one with the binding issue
        public ObservableCollection<EODSaleSummaryDto> TopSales
        {
            get => _topSales;
            set
            {
                _topSales = value;
                OnPropertyChanged(nameof(TopSales));
            }
        }
        public ObservableCollection<EODTopProductDto> TopSellingProducts
        {
            get => _topSellingProducts;
            set
            {
                _topSellingProducts = value;
                OnPropertyChanged(nameof(TopSellingProducts));
            }
        }


        public ICommand GenerateReportCommand { get; }
        public ICommand PrintReportCommand { get; }
        public ICommand PrintToPdfCommand { get; }
        public ICommand CloseDayCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        private async Task GenerateReport()
        {
            IsLoading = true;
            Message = "Generating report...";
            try
            {
                Report = await _service.GetEODReportAsync(ReportDate);
                // OpeningCash = Report?.OpeningCash ?? 0; // Removed incorrect assignment
                ExpectedCash = OpeningCash + CashSalesAmount - CashRefundAmount - TotalExpenses;
                CalculateCashDifference();
                Message = "Report generated successfully.";
            }
            catch (Exception ex)
            {
                Report = null;
                Message = $"Error generating report: {ex.Message}";
                DialogService.Error("Report Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PrintReport()
        {
            if (Report == null) return;
            _printService?.PrintEODReport(Report, ReportDate, OpeningCash, ActualCash, ExpectedCash, CashDifference);
            Message = "Print requested.";
        }

        private void PrintToPdf()
        {
            if (Report == null) return;
            try
            {
                _pdfExportService?.ExportEODReportToPdf(Report, ReportDate, OpeningCash, ActualCash, ExpectedCash, CashDifference);
                Message = "PDF export requested.";
            }
            catch (Exception ex)
            {
                DialogService.Error("PDF Export", ex.Message);
            }
        }

        private async Task CloseDay()
        {
            IsLoading = true;
            Message = "Closing day...";
            try
            {
                await _service.CloseDayAsync(ReportDate);
                Message = "Day closed successfully.";
                DialogService.Info("Day Close", "Day has been successfully closed.");
                await GenerateReport(); // Regenerate report after closing
            }
            catch (Exception ex)
            {
                Message = $"Error closing day: {ex.Message}";
                DialogService.Error("Day Close Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
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

        private void CalculateCashDifference()
        {
            CashDifference = ActualCash - ExpectedCash;
            OnPropertyChanged(nameof(CashDifference));
            OnPropertyChanged(nameof(IsCashShortage));
            OnPropertyChanged(nameof(IsCashOverage));
        }

        // Nested class for Payment Breakdown UI binding
        public class PaymentBreakdownItem : ViewModelBase
        {
            private string _method = string.Empty;
            public string Method
            {
                get => _method;
                set
                {
                    _method = value;
                    OnPropertyChanged(nameof(Method));
                }
            }

            private decimal _amount;
            public decimal Amount
            {
                get => _amount;
                set
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                    OnPropertyChanged(nameof(AmountFormatted));
                }
            }

            public string AmountFormatted => Amount.ToString("N2");
        }
    }
}