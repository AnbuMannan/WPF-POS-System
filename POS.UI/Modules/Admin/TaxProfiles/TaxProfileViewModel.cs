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

namespace POS.UI.Modules.Admin.TaxProfiles
{
    public class TaxProfileViewModel : ViewModelBase
    {
        private readonly TaxProfileApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        public ObservableCollection<TaxProfileDto> TaxProfiles { get; set; } = new();
        private List<TaxProfileDto> _allTaxProfiles = new();

        private TaxProfileDto _selectedTaxProfile;
        public TaxProfileDto SelectedTaxProfile
        {
            get => _selectedTaxProfile;
            set
            {
                _selectedTaxProfile = value;
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
                // Live search with debounce (search while typing)
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
                _ = LoadAsync(); // Reload from API with includeInactive so list has correct data
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        public TaxProfileViewModel(TaxProfileApiService service)
        {
            _service = service;

            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(400)
            };
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
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedTaxProfile != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedTaxProfile != null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_service == null)
                return;
            try
            {
                var list = await _service.GetAllAsync(ShowInactive);
                _allTaxProfiles = list ?? new List<TaxProfileDto>();
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load tax profiles", ex.Message);
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
            if (_allTaxProfiles == null)
                _allTaxProfiles = new List<TaxProfileDto>();
            var filtered = _allTaxProfiles.AsEnumerable();
            if (!ShowInactive)
                filtered = filtered.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            TaxProfiles.Clear();
            foreach (var item in filtered.ToList())
                TaxProfiles.Add(item);
        }

        private async Task AddAsync()
        {
            var form = new TaxProfileFormView(null);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        private async Task EditAsync()
        {
            if (SelectedTaxProfile == null)
                return;

            var form = new TaxProfileFormView(SelectedTaxProfile);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        private async Task DisableAsync()
        {
            if (SelectedTaxProfile == null)
                return;

            var result = POS.UI.Components.DialogService.Confirm("Confirm Disable", $"Disable tax profile '{SelectedTaxProfile.Name}'?");

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _service.DisableAsync(SelectedTaxProfile.TaxProfileId);
                POS.UI.Components.DialogService.Info("Success", "Tax profile disabled successfully");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Disable failed", ex.Message);
            }
        }
    }
}
