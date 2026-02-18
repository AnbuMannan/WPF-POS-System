using System.Collections.ObjectModel;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Reports.Finance
{
    public class ProfitLossSegment
    {
        public string Label { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ProfitLossReportViewModel : ViewModelBase
    {
        private readonly ReportApiService _api;
        private readonly IPrintService? _printService;

        private DateTime _fromDate = DateTime.Today.AddMonths(-1);
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PeriodText));
            }
        }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PeriodText));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                RaiseCommandsCanExecuteChanged();
            }
        }

        private ProfitLossReportDto? _report;
        public ProfitLossReportDto? Report
        {
            get => _report;
            set
            {
                _report = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalSales));
                OnPropertyChanged(nameof(TotalCogs));
                OnPropertyChanged(nameof(TotalExpenses));
                OnPropertyChanged(nameof(ProfitLoss));
                OnPropertyChanged(nameof(IsProfit));
                OnPropertyChanged(nameof(HasReport));
                OnPropertyChanged(nameof(PeriodText));
                BuildSegments();
                RaiseCommandsCanExecuteChanged();
            }
        }

        public decimal TotalSales => Report?.TotalSales ?? 0;
        public decimal TotalCogs => Report?.TotalCogs ?? 0;
        public decimal TotalExpenses => Report?.TotalExpenses ?? 0;
        public decimal ProfitLoss => Report?.ProfitLoss ?? 0;
        public bool IsProfit => ProfitLoss >= 0;
        public bool HasReport => Report != null;

        private decimal _maxSegmentAmount;
        public decimal MaxSegmentAmount
        {
            get => _maxSegmentAmount;
            private set
            {
                _maxSegmentAmount = value;
                OnPropertyChanged();
            }
        }

        public string PeriodText
        {
            get
            {
                if (Report != null)
                    return $"{Report.From:dd-MMM-yyyy} to {Report.To:dd-MMM-yyyy}";
                return $"{FromDate:dd-MMM-yyyy} to {ToDate:dd-MMM-yyyy}";
            }
        }

        public ObservableCollection<ProfitLossSegment> Segments { get; } = new();

        public ICommand GenerateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand PrintCommand { get; }

        public ProfitLossReportViewModel(ReportApiService api, IPrintService? printService = null)
        {
            _api = api;
            _printService = printService;
            GenerateCommand = new RelayCommand(async () => await GenerateAsync(), () => !IsLoading);
            ExportCommand = new RelayCommand(ExportToCsv, () => HasReport && !IsLoading);
            PrintCommand = new RelayCommand(Print, () => HasReport);
        }

        private async Task GenerateAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _api.GetProfitLossAsync(FromDate, ToDate);
                Report = result;
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Profit & Loss Report", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BuildSegments()
        {
            Segments.Clear();

            if (Report == null)
            {
                MaxSegmentAmount = 0;
                return;
            }

            var segments = new List<ProfitLossSegment>
            {
                new ProfitLossSegment { Label = "Sales", Amount = Report.TotalSales },
                new ProfitLossSegment { Label = "COGS", Amount = Report.TotalCogs },
                new ProfitLossSegment { Label = "Expenses", Amount = Report.TotalExpenses },
                new ProfitLossSegment
                {
                    Label = Report.ProfitLoss >= 0 ? "Profit" : "Loss",
                    Amount = Math.Abs(Report.ProfitLoss)
                }
            };

            MaxSegmentAmount = segments.Max(s => s.Amount);

            foreach (var segment in segments)
            {
                if (segment.Amount > 0)
                    Segments.Add(segment);
            }
        }

        private void ExportToCsv()
        {
            if (Report == null)
                return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"ProfitLoss_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}.csv"
            };

            if (dialog.ShowDialog() != true)
                return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Profit & Loss Report");
            sb.AppendLine($"From,{Report.From:yyyy-MM-dd}");
            sb.AppendLine($"To,{Report.To:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("Metric,Amount");
            sb.AppendLine($"Total Sales,{Report.TotalSales}");
            sb.AppendLine($"Cost of Goods Sold,{Report.TotalCogs}");
            sb.AppendLine($"Total Expenses,{Report.TotalExpenses}");
            sb.AppendLine($"Net {(Report.ProfitLoss >= 0 ? "Profit" : "Loss")},{Report.ProfitLoss}");

            System.IO.File.WriteAllText(dialog.FileName, sb.ToString());
        }

        private void Print()
        {
            if (Report == null)
                return;

            if (_printService == null)
            {
                POS.UI.Components.DialogService.Error("Print", "Print service is not available.");
                return;
            }

            try
            {
                _printService.PrintProfitLossReport(Report);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Print Profit & Loss Report", ex.Message);
            }
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            if (GenerateCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
            if (ExportCommand is RelayCommand rc2) rc2.RaiseCanExecuteChanged();
            if (PrintCommand is RelayCommand rc3) rc3.RaiseCanExecuteChanged();
        }
    }
}

