using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.Modules.Dashboard;

public class DashboardViewModel : ViewModelBase
{
    private readonly DashboardApiService _service;

    public DashboardViewModel(DashboardApiService service)
    {
        _service = service;
        RefreshCommand = new RelayCommand(async () => await InitializeAsync());
    }

    private DashboardSummaryDto? _summary;
    public DashboardSummaryDto? Summary
    {
        get => _summary;
        set
        {
            _summary = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TodaySalesAmountDisplay));
            OnPropertyChanged(nameof(TodayTransactionCountDisplay));
            OnPropertyChanged(nameof(LowStockItemCountDisplay));
            OnPropertyChanged(nameof(PendingOrdersCountDisplay));
            RecentSales.Clear();
            if (_summary?.RecentSales != null)
            {
                foreach (var s in _summary.RecentSales)
                    RecentSales.Add(s);
            }
        }
    }

    public ObservableCollection<DashboardRecentSaleDto> RecentSales { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public string TodaySalesAmountDisplay => Summary?.TodaySalesAmount.ToString("N2") ?? "0.00";
    public string TodayTransactionCountDisplay => (Summary?.TodayTransactionCount ?? 0).ToString();
    public string LowStockItemCountDisplay => (Summary?.LowStockItemCount ?? 0).ToString();
    public string PendingOrdersCountDisplay => (Summary?.PendingOrdersCount ?? 0).ToString();

    public ICommand RefreshCommand { get; }

    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            var summary = await _service.GetSummaryAsync(DateTime.Today);
            Summary = summary ?? new DashboardSummaryDto();
        }
        catch (Exception ex)
        {
            // For now, just expose via Summary with zeroed values
            Summary = new DashboardSummaryDto();
        }
        finally
        {
            IsLoading = false;
        }
    }
}

