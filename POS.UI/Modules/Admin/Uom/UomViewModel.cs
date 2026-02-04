using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace POS.UI.Modules.Admin.Uom
{
    public class UomViewModel : ViewModelBase
    {
        private readonly UomApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        public ObservableCollection<UomDto> Uoms { get; set; } = new();
        private List<UomDto> _allUoms = new();

        private UomDto _selectedUom;
        public UomDto SelectedUom
        {
            get => _selectedUom;
            set
            {
                _selectedUom = value;
                OnPropertyChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }

        private bool _showInactive;
        public bool ShowInactive
        {
            get => _showInactive;
            set
            {
                _showInactive = value;
                OnPropertyChanged();
                _ = LoadAsync();
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        public UomViewModel(UomApiService service)
        {
            _service = service;
            _searchTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyDisplayFilter();
            };

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(ApplySearch);
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ClearCommand = new RelayCommand(ClearSearch);
            AddCommand = new RelayCommand(async () => await AddAsync());
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedUom != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedUom != null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_service == null)
            {
                _allUoms = new List<UomDto>();
                ApplyDisplayFilter();
                return;
            }
            try
            {
                var list = await _service.GetAllAsync(ShowInactive);
                _allUoms = list ?? new List<UomDto>();
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load UoM", ex.Message);
            }
        }

        private void ApplySearch() => ApplyDisplayFilter();

        private void ClearSearch()
        {
            SearchText = string.Empty;
            ApplyDisplayFilter();
        }

        private void ApplyDisplayFilter()
        {
            var filtered = _allUoms.AsEnumerable();
            if (!ShowInactive)
                filtered = filtered.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(x =>
                    (x.Name != null && x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Code != null && x.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

            Uoms.Clear();
            foreach (var item in filtered.ToList())
                Uoms.Add(item);
        }

        private async Task AddAsync()
        {
            if (_service == null) { POS.UI.Components.DialogService.Warning("UoM", "Service not available."); return; }
            var form = new UomFormView(null);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        private async Task EditAsync()
        {
            if (SelectedUom == null) return;
            if (_service == null) { POS.UI.Components.DialogService.Warning("UoM", "Service not available."); return; }
            var form = new UomFormView(SelectedUom);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        private async Task DisableAsync()
        {
            if (SelectedUom == null) return;
            if (_service == null) { POS.UI.Components.DialogService.Warning("UoM", "Service not available."); return; }
            var result = POS.UI.Components.DialogService.Confirm("Confirm Disable", $"Disable UoM '{SelectedUom.Name}'?");
            if (result != MessageBoxResult.Yes) return;
            try
            {
                await _service.DisableAsync(SelectedUom.Id);
                POS.UI.Components.DialogService.Info("Success", "UoM disabled successfully");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Disable failed", ex.Message);
            }
        }
    }
}
