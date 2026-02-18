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

namespace POS.UI.Modules.Suppliers.SupplierList
{
    public class SupplierViewModel : ViewModelBase
    {
        private readonly SupplierApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<SupplierDto> Suppliers { get; set; } = new();

        private List<SupplierDto> _allSuppliers = new();

        // ================= SELECTION =================

        private SupplierDto _selectedSupplier;
        public SupplierDto SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= SEARCH =================

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

        // ================= COMMANDS =================

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        // ================= CONSTRUCTOR =================

        public SupplierViewModel(SupplierApiService service)
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
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedSupplier != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedSupplier != null);

            _ = LoadAsync();
        }

        // ================= LOAD LIST =================

        private async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync(ShowInactive);
                _allSuppliers = list ?? new List<SupplierDto>();
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load suppliers", ex.Message);
            }
        }

        // ================= SEARCH & DISPLAY FILTER =================

        private void ApplySearch() => ApplyDisplayFilter();

        private void ClearSearch()
        {
            SearchText = string.Empty;
            ApplyDisplayFilter();
        }

        private void ApplyDisplayFilter()
        {
            var filtered = _allSuppliers.AsEnumerable();
            if (!ShowInactive)
                filtered = filtered.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || x.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || (x.ContactPerson != null && x.ContactPerson.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    || (x.Mobile != null && x.Mobile.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

            Suppliers.Clear();
            foreach (var item in filtered.ToList())
                Suppliers.Add(item);
        }

        // ================= ADD =================

        private async Task AddAsync()
        {
            var form = new SupplierFormView(null);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        // ================= EDIT =================

        private async Task EditAsync()
        {
            if (SelectedSupplier == null)
                return;

            var form = new SupplierFormView(SelectedSupplier);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        // ================= DISABLE (SOFT DELETE) =================

        private async Task DisableAsync()
        {
            if (SelectedSupplier == null)
                return;

            var result = POS.UI.Components.DialogService.Confirm("Confirm Disable", $"Disable supplier '{SelectedSupplier.Name}' ?");

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _service.DisableAsync(SelectedSupplier.Id);

                POS.UI.Components.DialogService.Info("Success", "Supplier disabled successfully");

                await LoadAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Disable failed", ex.Message);
            }
        }
    }
}
