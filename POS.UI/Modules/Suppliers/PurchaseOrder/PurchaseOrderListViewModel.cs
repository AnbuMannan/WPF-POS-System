using POS.Shared.Models;
using POS.Shared.Enums;
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

namespace POS.UI.Modules.Suppliers.PurchaseOrder
{
    public class PurchaseOrderListViewModel : ViewModelBase
    {
        private readonly PurchaseOrderApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<PurchaseOrderDto> PurchaseOrders { get; set; } = new();

        private List<PurchaseOrderDto> _allPurchaseOrders = new();

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

        private PurchaseOrderDto _selectedPurchaseOrder;
        public PurchaseOrderDto SelectedPurchaseOrder
        {
            get => _selectedPurchaseOrder;
            set
            {
                _selectedPurchaseOrder = value;
                OnPropertyChanged();
                ((RelayCommand)ViewCommand).RaiseCanExecuteChanged();
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DisableCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateStatusCommand).RaiseCanExecuteChanged();
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

        private PurchaseOrderStatus? _filterStatus;
        public PurchaseOrderStatus? FilterStatus
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
        public ICommand DisableCommand { get; }
        public ICommand UpdateStatusCommand { get; }

        // ================= CONSTRUCTOR =================

        public PurchaseOrderListViewModel(PurchaseOrderApiService service)
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
            ViewCommand = new RelayCommand(() => ViewPurchaseOrder(), () => SelectedPurchaseOrder != null);
            EditCommand = new RelayCommand(() => EditPurchaseOrder(), () => SelectedPurchaseOrder != null && SelectedPurchaseOrder.Status == PurchaseOrderStatus.Draft);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedPurchaseOrder != null);
            UpdateStatusCommand = new RelayCommand(() => UpdateStatus(), () => SelectedPurchaseOrder != null);

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
                _allPurchaseOrders = data ?? new List<PurchaseOrderDto>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Warning("Error", $"Failed to load purchase orders.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allPurchaseOrders.AsEnumerable();

            // Apply status filter
            if (FilterStatus.HasValue)
            {
                filtered = filtered.Where(po => po.Status == FilterStatus.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(po =>
                    (po.SupplierName?.ToLower().Contains(search) ?? false) ||
                    (po.SupplierCode?.ToLower().Contains(search) ?? false) ||
                    (po.ReferenceNo?.ToLower().Contains(search) ?? false) ||
                    po.PurchaseOrderId.ToString().ToLower().Contains(search));
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                PurchaseOrders.Clear();
                foreach (var po in filtered)
                {
                    PurchaseOrders.Add(po);
                }
            });
        }

        private void ShowCreateForm()
        {
            var createViewModel = new CreatePurchaseOrderViewModel(_service);
            createViewModel.OnSaved += async () => await LoadAsync();
            
            var createView = new CreatePurchaseOrderView { DataContext = createViewModel };
            var window = new Window
            {
                Title = "Create Purchase Order",
                Content = createView,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            window.ShowDialog();
        }

        private void ViewPurchaseOrder()
        {
            if (SelectedPurchaseOrder == null) return;

            var viewViewModel = new CreatePurchaseOrderViewModel(_service, SelectedPurchaseOrder.PurchaseOrderId, isReadOnly: true);
            
            var viewView = new CreatePurchaseOrderView { DataContext = viewViewModel };
            var window = new Window
            {
                Title = $"View Purchase Order - {SelectedPurchaseOrder.ReferenceNo ?? SelectedPurchaseOrder.PurchaseOrderId.ToString()}",
                Content = viewView,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            window.ShowDialog();
        }

        private void EditPurchaseOrder()
        {
            if (SelectedPurchaseOrder == null || SelectedPurchaseOrder.Status != PurchaseOrderStatus.Draft) return;

            var editViewModel = new CreatePurchaseOrderViewModel(_service, SelectedPurchaseOrder.PurchaseOrderId);
            editViewModel.OnSaved += async () => await LoadAsync();
            
            var editView = new CreatePurchaseOrderView { DataContext = editViewModel };
            var window = new Window
            {
                Title = $"Edit Purchase Order - {SelectedPurchaseOrder.ReferenceNo ?? SelectedPurchaseOrder.PurchaseOrderId.ToString()}",
                Content = editView,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };
            window.ShowDialog();
        }

        private async Task DisableAsync()
        {
            if (SelectedPurchaseOrder == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete purchase order '{SelectedPurchaseOrder.ReferenceNo ?? SelectedPurchaseOrder.PurchaseOrderId.ToString()}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    await _service.DisableAsync(SelectedPurchaseOrder.PurchaseOrderId);
                    POS.UI.Components.DialogService.Success("Success", "Purchase order deleted successfully.");
                    await LoadAsync();
                }
                catch (Exception ex)
                {
                    POS.UI.Components.DialogService.Warning("Error", $"Failed to delete purchase order.\n{ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void UpdateStatus()
        {
            if (SelectedPurchaseOrder == null) return;

            // Show status update dialog
            var dialog = new StatusUpdateDialog(SelectedPurchaseOrder.Status);
            if (dialog.ShowDialog() == true)
            {
                var newStatus = dialog.SelectedStatus;
                Task.Run(async () =>
                {
                    try
                    {
                        IsBusy = true;
                        await _service.UpdateStatusAsync(SelectedPurchaseOrder.PurchaseOrderId, newStatus);
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            POS.UI.Components.DialogService.Success("Success", "Status updated successfully.");
                        });
                        await LoadAsync();
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            POS.UI.Components.DialogService.Warning("Error", $"Failed to update status.\n{ex.Message}");
                        });
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                });
            }
        }
    }

    // Simple status update dialog
    public class StatusUpdateDialog : Window
    {
        public PurchaseOrderStatus SelectedStatus { get; private set; }

        public StatusUpdateDialog(PurchaseOrderStatus currentStatus)
        {
            Title = "Update Status";
            Width = 300;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(20) };

            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Select new status:",
                Margin = new Thickness(0, 0, 0, 10)
            });

            var combo = new System.Windows.Controls.ComboBox
            {
                ItemsSource = Enum.GetValues(typeof(PurchaseOrderStatus)),
                SelectedItem = currentStatus,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "Update",
                Padding = new Thickness(20, 5, 20, 5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            okButton.Click += (s, e) =>
            {
                SelectedStatus = (PurchaseOrderStatus)combo.SelectedItem;
                DialogResult = true;
                Close();
            };

            stack.Children.Add(combo);
            stack.Children.Add(okButton);

            Content = stack;
        }
    }
}
