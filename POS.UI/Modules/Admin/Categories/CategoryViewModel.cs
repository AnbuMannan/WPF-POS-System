using POS.UI.Core.Models;
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

namespace POS.UI.Modules.Admin.Categories
{
    public class CategoryViewModel : ViewModelBase
    {
        private readonly CategoryApiService _service;

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
            }
        }

        // ================= COMMANDS =================

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddRootCommand { get; }
        public ICommand AddSubCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DisableCommand { get; }

        // ================= CONSTRUCTOR =================

        public CategoryViewModel(CategoryApiService service)
        {
            _service = service;

            LoadCommand = new RelayCommand(async () => await LoadAsync());
            SearchCommand = new RelayCommand(ApplySearch);

            AddRootCommand = new RelayCommand(AddRoot);
            AddSubCommand = new RelayCommand(AddSub, () => SelectedCategory != null);
            EditCommand = new RelayCommand(Edit, () => SelectedCategory != null);
            DisableCommand = new RelayCommand(async () => await DisableAsync(), () => SelectedCategory != null);

            // 🔥 Auto load when screen opens
            _ = LoadAsync();
        }

        // ================= LOAD LIST =================

        private async Task LoadAsync()
        {
            try
            {
                var list = await _service.GetAllAsync();

                _allCategories = list;

                Categories.Clear();
                foreach (var item in list)
                    Categories.Add(item);
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load categories", ex.Message);
            }
        }

        // ================= SEARCH =================

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Categories.Clear();
                foreach (var item in _allCategories)
                    Categories.Add(item);

                return;
            }

            var filtered = _allCategories
                .Where(x => x.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                         || (x.ParentCategoryName != null && x.ParentCategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Categories.Clear();
            foreach (var item in filtered)
                Categories.Add(item);
        }

        // ================= ADD ROOT =================

        private void AddRoot()
        {
            var form = new CategoryFormView(null);   // 🔥 Root category

            if (form.ShowDialog() == true)
            {
                _ = LoadAsync();   // 🔥 Refresh list after save
            }
        }

        // ================= ADD SUB =================

        private void AddSub()
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

            if (form.ShowDialog() == true)
            {
                _ = LoadAsync();
            }
        }

        // ================= EDIT =================

        private void Edit()
        {
            if (SelectedCategory == null)
                return;

            // 🔥 Pass full selected DTO
            var form = new CategoryFormView(SelectedCategory);

             if (form.ShowDialog() == true)
            {
                _ = LoadAsync();
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
