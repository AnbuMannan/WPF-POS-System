using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace POS.UI.Modules.Suppliers.SupplierLedger
{
    public class SupplierLedgerViewModel : ViewModelBase
    {
        private readonly SupplierApiService _supplierService;

        // ================= COLLECTIONS =================

        public ObservableCollection<SupplierDto> Suppliers { get; set; } = new();
        public ObservableCollection<SupplierLedgerEntryDto> LedgerEntries { get; set; } = new();

        // ================= FILTERS =================

        private SupplierDto? _selectedSupplier;
        public SupplierDto? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSupplierSelected));
                ((RelayCommand)ShowLedgerCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsSupplierSelected => SelectedSupplier != null;

        private DateTime _fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // First day of current month
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _toDate = DateTime.Now;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        // ================= REPORT DATA =================

        private SupplierLedgerReportDto? _currentReport;
        public SupplierLedgerReportDto? CurrentReport
        {
            get => _currentReport;
            set
            {
                _currentReport = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasReport));
                OnPropertyChanged(nameof(OpeningBalanceText));
                OnPropertyChanged(nameof(ClosingBalanceText));
                OnPropertyChanged(nameof(TotalDebitText));
                OnPropertyChanged(nameof(TotalCreditText));
                OnPropertyChanged(nameof(SupplierInfoText));
                OnPropertyChanged(nameof(DateRangeText));
                ((RelayCommand)PrintPdfCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ExportExcelCommand).RaiseCanExecuteChanged();
            }
        }

        public bool HasReport => CurrentReport != null && CurrentReport.Entries.Any();

        public string OpeningBalanceText => CurrentReport != null ? $"₹ {CurrentReport.OpeningBalance:N2}" : "₹ 0.00";
        public string ClosingBalanceText => CurrentReport != null ? $"₹ {CurrentReport.ClosingBalance:N2}" : "₹ 0.00";
        public string TotalDebitText => CurrentReport != null ? $"₹ {CurrentReport.TotalDebit:N2}" : "₹ 0.00";
        public string TotalCreditText => CurrentReport != null ? $"₹ {CurrentReport.TotalCredit:N2}" : "₹ 0.00";
        public string SupplierInfoText => CurrentReport != null 
            ? $"{CurrentReport.SupplierName} ({CurrentReport.SupplierCode})" 
            : "Select a supplier";
        public string DateRangeText => CurrentReport != null 
            ? $"{CurrentReport.FromDate:dd-MMM-yyyy} to {CurrentReport.ToDate:dd-MMM-yyyy}" 
            : string.Empty;

        // ================= LOADING STATE =================

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand)ShowLedgerCommand).RaiseCanExecuteChanged();
            }
        }

        private string _statusMessage = "Select a supplier and date range, then click 'Show Ledger'";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // ================= COMMANDS =================

        public ICommand ShowLedgerCommand { get; }
        public ICommand PrintPdfCommand { get; }
        public ICommand ExportExcelCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ThisMonthCommand { get; }
        public ICommand LastMonthCommand { get; }
        public ICommand ThisYearCommand { get; }
        public ICommand Last30DaysCommand { get; }

        // ================= CONSTRUCTOR =================

        public SupplierLedgerViewModel(SupplierApiService supplierService)
        {
            _supplierService = supplierService;

            ShowLedgerCommand = new RelayCommand(async () => await LoadLedgerAsync(), CanShowLedger);
            PrintPdfCommand = new RelayCommand(PrintPdf, () => HasReport);
            ExportExcelCommand = new RelayCommand(async () => await ExportExcelAsync(), () => HasReport);
            ClearCommand = new RelayCommand(ClearReport);
            
            // Quick date range commands
            ThisMonthCommand = new RelayCommand(SetThisMonth);
            LastMonthCommand = new RelayCommand(SetLastMonth);
            ThisYearCommand = new RelayCommand(SetThisYear);
            Last30DaysCommand = new RelayCommand(SetLast30Days);

            _ = LoadSuppliersAsync();
        }

        // ================= LOAD SUPPLIERS =================

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierService.GetAllAsync(false);
                Suppliers.Clear();
                foreach (var s in suppliers.OrderBy(x => x.Name))
                    Suppliers.Add(s);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Load Failed", $"Failed to load suppliers: {ex.Message}");
            }
        }

        // ================= SHOW LEDGER =================

        private bool CanShowLedger()
        {
            return SelectedSupplier != null && !IsLoading;
        }

        private async Task LoadLedgerAsync()
        {
            if (SelectedSupplier == null)
                return;

            if (FromDate > ToDate)
            {
                POS.UI.Components.DialogService.Warning("Invalid Date Range", "From Date cannot be after To Date.");
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Loading ledger...";

                var report = await _supplierService.GetLedgerAsync(SelectedSupplier.Id, FromDate, ToDate);
                
                if (report == null)
                {
                    POS.UI.Components.DialogService.Warning("Not Found", "Supplier not found or no data available.");
                    return;
                }

                CurrentReport = report;
                
                LedgerEntries.Clear();
                foreach (var entry in report.Entries)
                    LedgerEntries.Add(entry);

                StatusMessage = report.Entries.Count > 0 
                    ? $"Showing {report.Entries.Count} transaction(s)" 
                    : "No transactions found for the selected period";
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Load Failed", $"Failed to load ledger: {ex.Message}");
                StatusMessage = "Error loading ledger";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ================= CLEAR =================

        private void ClearReport()
        {
            CurrentReport = null;
            LedgerEntries.Clear();
            StatusMessage = "Select a supplier and date range, then click 'Show Ledger'";
        }

        // ================= DATE RANGE PRESETS =================

        private void SetThisMonth()
        {
            FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ToDate = DateTime.Now;
        }

        private void SetLastMonth()
        {
            var lastMonth = DateTime.Now.AddMonths(-1);
            FromDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            ToDate = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        }

        private void SetThisYear()
        {
            // Financial year in India: April to March
            var now = DateTime.Now;
            var fyStart = now.Month >= 4 
                ? new DateTime(now.Year, 4, 1) 
                : new DateTime(now.Year - 1, 4, 1);
            FromDate = fyStart;
            ToDate = DateTime.Now;
        }

        private void SetLast30Days()
        {
            FromDate = DateTime.Now.AddDays(-30);
            ToDate = DateTime.Now;
        }

        // ================= PRINT PDF =================

        private void PrintPdf()
        {
            if (CurrentReport == null)
                return;

            try
            {
                // Generate simple text-based print preview
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine("                         SUPPLIER LEDGER REPORT                    ");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine($"Supplier: {CurrentReport.SupplierName} ({CurrentReport.SupplierCode})");
                sb.AppendLine($"Contact: {CurrentReport.ContactPerson} | Mobile: {CurrentReport.Mobile}");
                sb.AppendLine($"Period: {CurrentReport.FromDate:dd-MMM-yyyy} to {CurrentReport.ToDate:dd-MMM-yyyy}");
                sb.AppendLine();
                sb.AppendLine($"Opening Balance: ₹ {CurrentReport.OpeningBalance:N2}");
                sb.AppendLine();
                sb.AppendLine("───────────────────────────────────────────────────────────────────");
                sb.AppendLine($"{"Date",-12} {"Description",-25} {"Ref No",-15} {"Debit",12} {"Credit",12} {"Balance",12}");
                sb.AppendLine("───────────────────────────────────────────────────────────────────");

                foreach (var entry in CurrentReport.Entries)
                {
                    var desc = entry.Description.Length > 23 ? entry.Description.Substring(0, 23) + ".." : entry.Description;
                    var refNo = (entry.ReferenceNo ?? "-").Length > 13 ? entry.ReferenceNo!.Substring(0, 13) + ".." : (entry.ReferenceNo ?? "-");
                    sb.AppendLine($"{entry.Date:dd-MMM-yyyy} {desc,-25} {refNo,-15} {entry.DebitAmount,12:N2} {entry.CreditAmount,12:N2} {entry.RunningBalance,12:N2}");
                }

                sb.AppendLine("───────────────────────────────────────────────────────────────────");
                sb.AppendLine($"{"TOTALS",-54} {CurrentReport.TotalDebit,12:N2} {CurrentReport.TotalCredit,12:N2}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"Closing Balance: ₹ {CurrentReport.ClosingBalance:N2}");
                sb.AppendLine();
                sb.AppendLine($"Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm:ss}");

                // Show in a print dialog / preview
                var dialog = new Window
                {
                    Title = "Supplier Ledger - Print Preview",
                    Width = 900,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                var textBox = new System.Windows.Controls.TextBox
                {
                    Text = sb.ToString(),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 11,
                    IsReadOnly = true,
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    TextWrapping = System.Windows.TextWrapping.NoWrap
                };

                dialog.Content = textBox;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Print Failed", ex.Message);
            }
        }

        // ================= EXPORT EXCEL (CSV) =================

        private async Task ExportExcelAsync()
        {
            if (CurrentReport == null)
                return;

            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"SupplierLedger_{CurrentReport.SupplierCode}_{CurrentReport.FromDate:yyyyMMdd}_{CurrentReport.ToDate:yyyyMMdd}.csv",
                    Title = "Export Supplier Ledger"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                var sb = new StringBuilder();
                
                // Header info
                sb.AppendLine($"Supplier Ledger Report");
                sb.AppendLine($"Supplier,{CurrentReport.SupplierName} ({CurrentReport.SupplierCode})");
                sb.AppendLine($"Period,{CurrentReport.FromDate:dd-MMM-yyyy} to {CurrentReport.ToDate:dd-MMM-yyyy}");
                sb.AppendLine($"Opening Balance,{CurrentReport.OpeningBalance:N2}");
                sb.AppendLine();
                
                // Column headers
                sb.AppendLine("Date,Description,Transaction Type,Reference No,Debit,Credit,Running Balance");
                
                // Data rows
                foreach (var entry in CurrentReport.Entries)
                {
                    var desc = entry.Description.Replace(",", " ").Replace("\"", "'");
                    var refNo = (entry.ReferenceNo ?? "").Replace(",", " ").Replace("\"", "'");
                    sb.AppendLine($"{entry.Date:yyyy-MM-dd},{desc},{entry.TransactionType},{refNo},{entry.DebitAmount:N2},{entry.CreditAmount:N2},{entry.RunningBalance:N2}");
                }
                
                // Totals
                sb.AppendLine();
                sb.AppendLine($"Total Debit,{CurrentReport.TotalDebit:N2}");
                sb.AppendLine($"Total Credit,{CurrentReport.TotalCredit:N2}");
                sb.AppendLine($"Closing Balance,{CurrentReport.ClosingBalance:N2}");

                await File.WriteAllTextAsync(saveDialog.FileName, sb.ToString(), Encoding.UTF8);

                POS.UI.Components.DialogService.Info("Export Complete", $"Ledger exported to:\n{saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Export Failed", ex.Message);
            }
        }

        // ================= KEYBOARD SHORTCUTS =================

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5 && CanShowLedger())
            {
                _ = LoadLedgerAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.P && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control && HasReport)
            {
                PrintPdf();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.E && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control && HasReport)
            {
                _ = ExportExcelAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                ClearReport();
                e.Handled = true;
            }
        }
    }
}
