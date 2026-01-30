using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Customers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Admin.Customers
{
    public class CustomerViewModel : ViewModelBase
    {
        private readonly CustomerApiService _service;

        // ================= COLLECTIONS =================

        public ObservableCollection<CustomerDto> Customers { get; set; } = new();

        private List<CustomerDto> _allCustomers = new();

        // ================= SELECTION =================

        private CustomerDto _selectedCustomer;
        public CustomerDto SelectedCustomer
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

        // ================= SEARCH =================

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        // ================= COMMANDS =================

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        // ================= CONSTRUCTOR =================

        public CustomerViewModel(CustomerApiService service)
        {
            _service = service;

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(ApplySearch);
            RefreshCommand = new RelayCommand(async () => await LoadAsync());

            AddCommand = new RelayCommand(async () => await AddAsync());
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedCustomer != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedCustomer != null);

            _ = LoadAsync();
        }

        // ================= LOAD LIST =================

        private async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync();
                _allCustomers = list;

                Customers.Clear();
                foreach (var item in list)
                    Customers.Add(item);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load customers", ex.Message);
            }
        }

        // ================= SEARCH =================

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Customers.Clear();
                foreach (var item in _allCustomers)
                    Customers.Add(item);
                return;
            }

            var filtered = _allCustomers
                .Where(x => x.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                         || (x.Phone != null && x.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                         || (x.Email != null && x.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Customers.Clear();
            foreach (var item in filtered)
                Customers.Add(item);
        }

        // ================= ADD =================

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

        // ================= EDIT =================

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

        // ================= DISABLE (SOFT DELETE) =================

        private async Task DisableAsync()
        {
            if (SelectedCustomer == null)
                return;

            var result = POS.UI.Components.DialogService.Confirm("Confirm Disable", $"Disable customer '{SelectedCustomer.FullName}' ?");

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _service.DisableAsync(SelectedCustomer.CustomerId);

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
