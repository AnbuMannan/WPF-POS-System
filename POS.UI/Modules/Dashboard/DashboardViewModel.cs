using POS.Shared.Models;
using POS.UI.Core;
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
        RefreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsLoading);
        OnPropertyChanged(nameof(WelcomeText));
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
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            if (RefreshCommand is RelayCommand rc)
                rc.RaiseCanExecuteChanged();
        }
    }

    public string TodaySalesAmountDisplay => Summary?.TodaySalesAmount.ToString("N2") ?? "0.00";
    public string TodayTransactionCountDisplay => (Summary?.TodayTransactionCount ?? 0).ToString();
    public string LowStockItemCountDisplay => (Summary?.LowStockItemCount ?? 0).ToString();
    public string PendingOrdersCountDisplay => (Summary?.PendingOrdersCount ?? 0).ToString();

    public string WelcomeText =>
        string.IsNullOrWhiteSpace(AppState.CurrentUserName)
            ? "Welcome"
            : $"Welcome, {AppState.CurrentUserName}";

    public ICommand RefreshCommand { get; }

    public async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            var summary = await _service.GetSummaryAsync(DateTime.Today);
            Summary = summary ?? new DashboardSummaryDto();
        }
        catch
        {
            Summary = new DashboardSummaryDto();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        RecentSales.Clear();
        Summary = new DashboardSummaryDto();
        await InitializeAsync();
    }
}

