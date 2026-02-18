using System.Collections.ObjectModel;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Components;

namespace POS.UI.Modules.Reports.Sales
{
    public enum ReportType
    {
        SalesSummary,
        ItemWiseSales
    }

    public class ReportViewerViewModel : ViewModelBase
    {
        private readonly ReportApiService _api;

        public ReportType ReportType { get; }

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

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                RaiseCommandsCanExecuteChanged();
            }
        }

        public ObservableCollection<SalesSummaryReportRow> SalesRows { get; } = new();
        public ObservableCollection<ItemWiseSalesRow> ItemRows { get; } = new();

        public ICommand GenerateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand PrintCommand { get; }

        public ReportViewerViewModel(ReportApiService api, ReportType reportType)
        {
            _api = api;
            ReportType = reportType;
            GenerateCommand = new RelayCommand(async () => await GenerateAsync(), () => !IsBusy);
            ExportCommand = new RelayCommand(ExportToCsv, () => !IsBusy);
            PrintCommand = new RelayCommand(Print, () => !IsBusy);
        }

        private async Task GenerateAsync()
        {
            IsBusy = true;
            try
            {
                if (ReportType == ReportType.SalesSummary)
                {
                    SalesRows.Clear();
                    var rows = await _api.GetSalesReportAsync(FromDate, ToDate, null, null);
                    foreach (var r in rows)
                        SalesRows.Add(r);
                }
                else if (ReportType == ReportType.ItemWiseSales)
                {
                    ItemRows.Clear();
                    var rows = await _api.GetItemWiseSalesAsync(FromDate, ToDate, null);
                    foreach (var r in rows)
                        ItemRows.Add(r);
                }
            }
            catch (Exception ex)
            {
                var title = ReportType == ReportType.SalesSummary
                    ? "Sales Report"
                    : "Item-wise Sales Report";
                DialogService.Error(title, ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExportToCsv()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = ReportType == ReportType.SalesSummary
                    ? $"SalesReport_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}.csv"
                    : $"ItemSales_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}.csv"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            var sb = new System.Text.StringBuilder();
            if (ReportType == ReportType.SalesSummary)
            {
                sb.AppendLine("Date,InvoiceCount,Subtotal,TaxAmount,TotalAmount");
                foreach (var r in SalesRows)
                    sb.AppendLine($"{r.Date:yyyy-MM-dd},{r.InvoiceCount},{r.Subtotal},{r.TaxAmount},{r.TotalAmount}");
            }
            else
            {
                sb.AppendLine("ProductId,ProductName,CategoryName,QuantitySold,TotalAmount");
                foreach (var r in ItemRows)
                    sb.AppendLine($"{r.ProductId},\"{r.ProductName}\",\"{r.CategoryName}\",{r.QuantitySold},{r.TotalAmount}");
            }

            System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString());
        }

        private void Print()
        {
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            if (GenerateCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            if (ExportCommand is RelayCommand rc2) rc2.RaiseCanExecuteChanged();
            if (PrintCommand is RelayCommand rc3) rc3.RaiseCanExecuteChanged();
        }
    }
}
