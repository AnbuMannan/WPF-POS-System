using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Reports.Inventory
{
    public class LowStockReportViewModel : ViewModelBase
    {
        private readonly ReportApiService _api;

        private decimal _threshold = 0;
        public decimal Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                OnPropertyChanged();
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

        public ObservableCollection<LowStockItemRow> Items { get; } = new();

        public int TotalItems => Items.Count;

        public ICommand LoadCommand { get; }

        public LowStockReportViewModel(ReportApiService api)
        {
            _api = api;
            LoadCommand = new RelayCommand(async () => await LoadAsync(), () => !IsLoading);
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                Items.Clear();
                var result = await _api.GetLowStockAsync(Threshold);
                foreach (var row in result)
                {
                    Items.Add(row);
                }
                OnPropertyChanged(nameof(TotalItems));
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Low Stock Report", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            if (LoadCommand is RelayCommand rc) rc.RaiseCanExecuteChanged();
        }
    }
}
