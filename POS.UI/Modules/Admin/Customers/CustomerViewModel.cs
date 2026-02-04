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

namespace POS.UI.Modules.Admin.Customers
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly CustomerApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        public ObservableCollection<CustomerDto> Customers { get; set; } = new();
        private List<CustomerDto> _allCustomers = new();

        private CustomerDto? _selectedCustomer;
        public CustomerDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
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

        public CustomerViewModel(CustomerApiService service)
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
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedCustomer != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedCustomer != null);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync(ShowInactive);
                _allCustomers = list ?? new List<CustomerDto>();
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load customers", ex.Message);
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
            var filtered = _allCustomers.AsEnumerable();
            if (!ShowInactive)
                filtered = filtered.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(x =>
                    (x.Name != null && x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Phone != null && x.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Email != null && x.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

            Customers.Clear();
            foreach (var item in filtered.ToList())
                Customers.Add(item);
        }

        private async Task AddAsync()
        {
            var form = new CustomerFormView(null);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        private async Task EditAsync()
        {
            if (SelectedCustomer == null)
                return;

            var form = new CustomerFormView(SelectedCustomer);
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();
                ApplySearch();
            }
        }

        private async Task DisableAsync()
        {
            if (SelectedCustomer == null)
                return;

            var result = POS.UI.Components.DialogService.Confirm("Confirm Disable", $"Disable customer '{SelectedCustomer.Name}'?");

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _service.DisableAsync(SelectedCustomer.Id);
                POS.UI.Components.DialogService.Info("Success", "Customer disabled successfully");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Disable failed", ex.Message);
            }
        }
    }
}
