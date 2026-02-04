using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Categories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace POS.UI.Modules.Admin.Categories
{
    public class CategoryViewModel : ViewModelBase
    {
        private readonly CategoryApiService _service;
        private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

        // ================= COLLECTIONS =================

        public ObservableCollection<CategoryDto> Categories { get; set; } = new();

        private List<CategoryDto> _allCategories = new();   // 🔥 For search filtering

        // ================= SELECTION =================

        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();

                // 🔥 Enable / Disable buttons dynamically
                ((RelayCommand)AddSubCommand).RaiseCanExecuteChanged();
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
        public ICommand AddRootCommand { get; }
        public ICommand AddSubCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        // ================= CONSTRUCTOR =================

        public CategoryViewModel(CategoryApiService service)
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

            AddRootCommand = new RelayCommand(async () => await AddRootAsync());
            AddSubCommand = new RelayCommand(async () => await AddSubAsync(), () => SelectedCategory != null);
            EditCommand = new RelayCommand(async () => await EditAsync(), () => SelectedCategory != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedCategory != null);

            // 🔥 Auto load when screen opens
            _ = LoadAsync();
        }

        // ================= LOAD LIST =================

        private async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync(ShowInactive);

                // Fill ParentCategoryName if API didn't supply it
                var nameById = list.Where(c => c.CategoryId > 0)
                                   .DistinctBy(c => c.CategoryId)
                                   .ToDictionary(c => c.CategoryId, c => c.Name);
                foreach (var c in list)
                {
                    if (string.IsNullOrWhiteSpace(c.ParentCategoryName) && c.ParentCategoryId.HasValue && nameById.TryGetValue(c.ParentCategoryId.Value, out var pname))
                        c.ParentCategoryName = pname;
                }

                _allCategories = list ?? new List<CategoryDto>();
                ApplyDisplayFilter();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load categories", ex.Message);
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
            var filtered = _allCategories.AsEnumerable();
            if (!ShowInactive)
                filtered = filtered.Where(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || (x.ParentCategoryName != null && x.ParentCategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

            Categories.Clear();
            foreach (var item in filtered.ToList())
                Categories.Add(item);
        }

        // ================= ADD ROOT =================

        private async Task AddRootAsync()
        {
            var form = new CategoryFormView(null);   // 🔥 Root category
            form.Owner = Application.Current?.MainWindow;
            if (form.ShowDialog() == true)
            {
                await LoadAsync();   // 🔥 Refresh list after save
                ApplySearch();       // 🔥 Respect current search filter
            }
        }

        // ================= ADD SUB =================

        private async Task AddSubAsync()
        {
            if (SelectedCategory == null)
                return;

            // 🔥 Pre-fill parent info
            var dto = new CategoryDto
            {
                ParentCategoryId = SelectedCategory.CategoryId,
                ParentCategoryName = SelectedCategory.Name
            };

            var form = new CategoryFormView(dto);
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
            if (SelectedCategory == null)
                return;

            // 🔥 Pass full selected DTO
            var form = new CategoryFormView(SelectedCategory);
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
            if (SelectedCategory == null)
                return;

            var result = POS.UI.Components.DialogService.Confirm("Confirm Disable", $"Disable category '{SelectedCategory.Name}' ?");

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _service.DisableAsync(SelectedCategory.CategoryId);

                POS.UI.Components.DialogService.Info("Success", "Category disabled successfully");

                await LoadAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Disable failed", ex.Message);
            }
        }
    }
}
