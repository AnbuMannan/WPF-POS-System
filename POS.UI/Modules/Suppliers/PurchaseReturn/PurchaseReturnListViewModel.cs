using POS.Shared.Models;
using POS.UI.Components;
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
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace POS.UI.Modules.Suppliers.PurchaseReturn
{
    public class PurchaseReturnListViewModel : ViewModelBase
    {
        private readonly PurchaseReturnApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<PurchaseReturnDto> PurchaseReturns { get; set; } = new();

        private List<PurchaseReturnDto> _allPurchaseReturns = new();

        // ================= BUSY INDICATOR =================

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        // ================= SELECTION =================

        private PurchaseReturnDto? _selectedReturn;
        public PurchaseReturnDto? SelectedReturn
        {
            get => _selectedReturn;
            set
            {
                _selectedReturn = value;
                OnPropertyChanged();
                ((RelayCommand)ViewCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ProcessCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= SEARCH & FILTER =================

        private string _searchText = string.Empty;
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

        private string? _filterStatus;
        public string? FilterStatus
        {
            get => _filterStatus;
            set
            {
                _filterStatus = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        // ================= COMMANDS =================

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ProcessCommand { get; }
        public ICommand DisableCommand { get; }

        // ================= CONSTRUCTOR =================

        public PurchaseReturnListViewModel(PurchaseReturnApiService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            // Initialize search timer for debounce
            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyFilters();
            };

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(() => ApplyFilters());
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ClearCommand = new RelayCommand(() => { SearchText = string.Empty; FilterStatus = null; });
            AddCommand = new RelayCommand(() => ShowCreateForm());
            ViewCommand = new RelayCommand(() => ViewReturn(), () => SelectedReturn != null);
            EditCommand = new RelayCommand(() => EditReturn(), () => SelectedReturn != null && !SelectedReturn.IsProcessed);
            ProcessCommand = new RelayCommand(async () => await ProcessReturnAsync(), () => SelectedReturn != null && !SelectedReturn.IsProcessed);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedReturn != null && !SelectedReturn.IsProcessed);

            // Auto-load on creation
            _ = LoadAsync();
        }

        // ================= METHODS =================

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                var data = await _service.GetAllAsync(ShowInactive);
                _allPurchaseReturns = data ?? new List<PurchaseReturnDto>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogService.Error("Error", $"Failed to load purchase returns: {ex.Message}");
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allPurchaseReturns.AsEnumerable();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(pr =>
                    (pr.ReturnNo ?? "").ToLower().Contains(searchLower) ||
                    (pr.SupplierName ?? "").ToLower().Contains(searchLower) ||
                    (pr.Status ?? "").ToLower().Contains(searchLower));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(FilterStatus) && FilterStatus != "All Statuses")
            {
                filtered = filtered.Where(pr => (pr.Status ?? "").Equals(FilterStatus, StringComparison.OrdinalIgnoreCase));
            }

            PurchaseReturns.Clear();
            foreach (var item in filtered)
            {
                PurchaseReturns.Add(item);
            }
        }

        private void ShowCreateForm()
        {
            var createViewModel = new CreatePurchaseReturnViewModel(_service);
            createViewModel.OnSaved += async () => await LoadAsync();
            
            var createView = new CreatePurchaseReturnView { DataContext = createViewModel };
            var window = new Window
            {
                Title = "Create Purchase Return",
                Content = createView,
                Width = 1200,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            createViewModel.CloseAction = () => window.Close();
            window.ShowDialog();
        }

        private void ViewReturn()
        {
            if (SelectedReturn == null) return;

            var viewViewModel = new CreatePurchaseReturnViewModel(_service, SelectedReturn.Id, isReadOnly: true);
            
            var viewView = new CreatePurchaseReturnView { DataContext = viewViewModel };
            var window = new Window
            {
                Title = $"View Purchase Return - {SelectedReturn.ReturnNo}",
                Content = viewView,
                Width = 1200,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            viewViewModel.CloseAction = () => window.Close();
            window.ShowDialog();
        }

        private void EditReturn()
        {
            if (SelectedReturn == null || SelectedReturn.IsProcessed) return;

            var editViewModel = new CreatePurchaseReturnViewModel(_service, SelectedReturn.Id, isReadOnly: false);
            editViewModel.OnSaved += async () => await LoadAsync();
            
            var editView = new CreatePurchaseReturnView { DataContext = editViewModel };
            var window = new Window
            {
                Title = $"Edit Purchase Return - {SelectedReturn.ReturnNo}",
                Content = editView,
                Width = 1200,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            editViewModel.CloseAction = () => window.Close();
            window.ShowDialog();
        }

        private async Task ProcessReturnAsync()
        {
            if (SelectedReturn == null || SelectedReturn.IsProcessed) return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to process this purchase return?\n\nReturn No: {SelectedReturn.ReturnNo}\nSupplier: {SelectedReturn.SupplierName}\nTotal: ₹{SelectedReturn.TotalAmount:N2}\n\nThis will reduce stock and cannot be undone.",
                "Confirm Process",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                IsBusy = true;
                await _service.ProcessReturnAsync(SelectedReturn.Id);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogService.Success("Success", "Purchase return processed successfully. Stock has been reduced.");
                });

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogService.Error("Error", $"Failed to process purchase return: {ex.Message}");
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DisableAsync()
        {
            if (SelectedReturn == null) return;

            var confirm = MessageBox.Show(
                $"Are you sure you want to delete this purchase return?\n\nReturn No: {SelectedReturn.ReturnNo}",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                IsBusy = true;
                await _service.DisableAsync(SelectedReturn.Id);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogService.Success("Success", "Purchase return deleted successfully.");
                });

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DialogService.Error("Error", $"Failed to delete purchase return: {ex.Message}");
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
