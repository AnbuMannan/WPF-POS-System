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
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace POS.UI.Modules.Suppliers.PurchaseEntry
{
    public class PurchaseEntryListViewModel : ViewModelBase
    {
        private readonly PurchaseEntryApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<PurchaseEntryDto> PurchaseEntries { get; set; } = new();

        private List<PurchaseEntryDto> _allPurchaseEntries = new();

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

        private PurchaseEntryDto _selectedEntry;
        public PurchaseEntryDto SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();
                ((RelayCommand)ViewCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ProcessCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
            }
        }

        // ================= SEARCH & FILTER =================

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

        private bool _showOnlyUnprocessed;
        public bool ShowOnlyUnprocessed
        {
            get => _showOnlyUnprocessed;
            set
            {
                _showOnlyUnprocessed = value;
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

        public PurchaseEntryListViewModel(PurchaseEntryApiService service)
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
            ClearCommand = new RelayCommand(() => { SearchText = string.Empty; ShowOnlyUnprocessed = false; });
            AddCommand = new RelayCommand(() => ShowCreateForm());
            ViewCommand = new RelayCommand(() => ViewEntry(), () => SelectedEntry != null);
            EditCommand = new RelayCommand(() => EditEntry(), () => SelectedEntry != null && !SelectedEntry.IsProcessed);
            ProcessCommand = new RelayCommand(async () => await ProcessEntryAsync(), () => SelectedEntry != null && !SelectedEntry.IsProcessed);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedEntry != null && !SelectedEntry.IsProcessed);

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
                _allPurchaseEntries = data ?? new List<PurchaseEntryDto>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load purchase entries.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allPurchaseEntries.AsEnumerable();

            // Apply unprocessed filter
            if (ShowOnlyUnprocessed)
            {
                filtered = filtered.Where(pe => !pe.IsProcessed);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(pe =>
                    (pe.SupplierName?.ToLower().Contains(search) ?? false) ||
                    (pe.InvoiceNo?.ToLower().Contains(search) ?? false) ||
                    (pe.PurchaseOrderReferenceNo?.ToLower().Contains(search) ?? false) ||
                    pe.PurchaseEntryId.ToString().ToLower().Contains(search));
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                PurchaseEntries.Clear();
                foreach (var pe in filtered.OrderByDescending(p => p.ReceivedDate))
                {
                    PurchaseEntries.Add(pe);
                }
            });
        }

        private void ShowCreateForm()
        {
            var createViewModel = new CreatePurchaseEntryViewModel(_service);
            createViewModel.OnSaved += async () => await LoadAsync();
            
            var createView = new CreatePurchaseEntryView { DataContext = createViewModel };
            var window = new Window
            {
                Title = "Create Purchase Entry (GRN)",
                Content = createView,
                Width = 1200,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            window.ShowDialog();
        }

        private void ViewEntry()
        {
            if (SelectedEntry == null) return;

            var viewViewModel = new CreatePurchaseEntryViewModel(_service, SelectedEntry.PurchaseEntryId, isReadOnly: true);
            
            var viewView = new CreatePurchaseEntryView { DataContext = viewViewModel };
            var window = new Window
            {
                Title = $"View Purchase Entry - {SelectedEntry.InvoiceNo}",
                Content = viewView,
                Width = 1200,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            window.ShowDialog();
        }

        private void EditEntry()
        {
            if (SelectedEntry == null || SelectedEntry.IsProcessed) return;

            var editViewModel = new CreatePurchaseEntryViewModel(_service, SelectedEntry.PurchaseEntryId);
            editViewModel.OnSaved += async () => await LoadAsync();
            
            var editView = new CreatePurchaseEntryView { DataContext = editViewModel };
            var window = new Window
            {
                Title = $"Edit Purchase Entry - {SelectedEntry.InvoiceNo}",
                Content = editView,
                Width = 1200,
                Height = 750,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            window.ShowDialog();
        }

        private async Task ProcessEntryAsync()
        {
            if (SelectedEntry == null || SelectedEntry.IsProcessed) return;

            var result = MessageBox.Show(
                $"Are you sure you want to process purchase entry '{SelectedEntry.InvoiceNo}'?\n\n" +
                "This will:\n" +
                "• Update inventory (StockSummary)\n" +
                "• Create stock ledger entries\n" +
                "• Update product prices\n" +
                "• Update linked Purchase Order status\n\n" +
                "This action CANNOT be undone!",
                "Confirm Process",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _service.ProcessEntryAsync(SelectedEntry.PurchaseEntryId, updateProductPrices: true);
                    POS.UI.Components.DialogService.Success("Success", 
                        "Purchase entry processed successfully!\n\n" +
                        "Inventory has been updated.");
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    POS.UI.Components.DialogService.Warning("Error", 
                        $"Failed to process purchase entry.\n\n{ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task DisableAsync()
        {
            if (SelectedEntry == null) return;

            if (SelectedEntry.IsProcessed)
            {
                POS.UI.Components.DialogService.Warning("Cannot Delete", 
                    "Cannot delete a processed purchase entry.\n" +
                    "Inventory has already been updated.");
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete purchase entry '{SelectedEntry.InvoiceNo}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _service.DisableAsync(SelectedEntry.PurchaseEntryId);
                    POS.UI.Components.DialogService.Success("Success", "Purchase entry deleted successfully.");
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    POS.UI.Components.DialogService.Warning("Error", 
                        $"Failed to delete purchase entry.\n{ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
