using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.Modules.Inventory.StockAdjustment
{
    public class StockAdjustmentListViewModel : ViewModelBase
    {
        private readonly StockAdjustmentApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<StockAdjustmentDto> Adjustments { get; set; } = new();

        private List<StockAdjustmentDto> _allAdjustments = new();

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

        private StockAdjustmentDto? _selectedAdjustment;
        public StockAdjustmentDto? SelectedAdjustment
        {
            get => _selectedAdjustment;
            set
            {
                _selectedAdjustment = value;
                OnPropertyChanged();
                ((RelayCommand)ViewCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
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

        private string _selectedReasonFilter = "All";
        public string SelectedReasonFilter
        {
            get => _selectedReasonFilter;
            set
            {
                _selectedReasonFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public List<string> ReasonFilters { get; } = new List<string> 
        { 
            "All", 
            AdjustmentReasons.Damage, 
            AdjustmentReasons.Theft, 
            AdjustmentReasons.Expiry, 
            AdjustmentReasons.Correction, 
            AdjustmentReasons.Other 
        };

        // ================= COMMANDS =================

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand ViewCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearSearchCommand { get; }

        // ================= EVENTS =================

        public event Action? RequestAddNew;
        public event Action<StockAdjustmentDto>? RequestView;

        // ================= CONSTRUCTOR =================

        public StockAdjustmentListViewModel(StockAdjustmentApiService service)
        {
            _service = service;

            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyFilters();
            };

            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            AddCommand = new RelayCommand(() => RequestAddNew?.Invoke());
            ViewCommand = new RelayCommand(ViewAdjustment, () => SelectedAdjustment != null);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(), CanDelete);
            ClearSearchCommand = new RelayCommand(ClearSearch);

            _ = LoadAsync();
        }

        // ================= LOAD DATA =================

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                var list = await _service.GetAllAsync(ShowInactive);
                _allAdjustments = list ?? new List<StockAdjustmentDto>();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Load Failed", $"Failed to load adjustments: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allAdjustments.AsEnumerable();

            // Apply reason filter
            if (!string.IsNullOrEmpty(SelectedReasonFilter) && SelectedReasonFilter != "All")
            {
                filtered = filtered.Where(a => a.Reason == SelectedReasonFilter);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(a =>
                    a.ReferenceNo.ToLower().Contains(search) ||
                    a.Reason.ToLower().Contains(search) ||
                    (a.Remarks?.ToLower().Contains(search) ?? false) ||
                    (a.AdjustedBy?.ToLower().Contains(search) ?? false));
            }

            Adjustments.Clear();
            foreach (var item in filtered.OrderByDescending(a => a.AdjustmentDate).ThenByDescending(a => a.CreatedAt))
            {
                Adjustments.Add(item);
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedReasonFilter = "All";
            ApplyFilters();
        }

        // ================= ACTIONS =================

        private void ViewAdjustment()
        {
            if (SelectedAdjustment != null)
            {
                RequestView?.Invoke(SelectedAdjustment);
            }
        }

        private bool CanDelete()
        {
            return SelectedAdjustment != null && SelectedAdjustment.Status != AdjustmentStatus.Approved;
        }

        private async Task DeleteAsync()
        {
            if (SelectedAdjustment == null)
                return;

            if (SelectedAdjustment.Status == AdjustmentStatus.Approved)
            {
                POS.UI.Components.DialogService.Error("Cannot Delete", "Approved adjustments cannot be deleted.");
                return;
            }

            var confirm = POS.UI.Components.DialogService.Confirm(
                "Delete Adjustment",
                $"Are you sure you want to delete adjustment '{SelectedAdjustment.ReferenceNo}'?");

            if (confirm != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                IsBusy = true;
                await _service.DeleteAsync(SelectedAdjustment.Id);
                POS.UI.Components.DialogService.Success("Deleted", "Adjustment deleted successfully.");
                await LoadAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Delete Failed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ================= KEYBOARD SHORTCUTS =================

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                _ = LoadAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F2)
            {
                RequestAddNew?.Invoke();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Enter && SelectedAdjustment != null)
            {
                ViewAdjustment();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Delete && CanDelete())
            {
                _ = DeleteAsync();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.F && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                // Focus search - handled in view
                e.Handled = true;
            }
        }
    }
}
