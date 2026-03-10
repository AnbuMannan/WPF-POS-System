using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.UI.Modules.Admin.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;                 // 🔥 REQUIRED
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Control = System.Windows.Controls.Control;

namespace POS.UI.Modules.Admin.Categories
{
    public partial class CategoryFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly CategoryApiService _service;
        private bool _isEdit;
        private CategoryDto _editDto;
        private bool _isSaving;

        public ObservableCollection<LookupDto> Categories { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ---------------- VALIDATION ENGINE ----------------

        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        // Removed SKU/Barcode debounce fields; Category form does not validate product identifiers

        private void FocusFirstInvalidControl()
        {
            var firstInvalid = FindFirstInvalid(this);

            if (firstInvalid != null)
            {
                firstInvalid.BringIntoView();   // 🔥 Auto scroll
                firstInvalid.Focus();           // 🔥 Auto focus
            }
        }

        private Control? FindFirstInvalid(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Control control &&
                    System.Windows.Controls.Validation.GetHasError(control))
                {
                    return control;
                }

                var result = FindFirstInvalid(child);
                if (result != null)
                    return result;
            }

            return null;
        }
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }


        // ---------------- BASIC FIELDS ----------------

        private string _categoryName = string.Empty;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged(nameof(CategoryName));
                ClearErrors(nameof(CategoryName));
                if (string.IsNullOrWhiteSpace(CategoryName))
                    AddError(nameof(CategoryName), "Category name is required");
            }
        }

        private string _parentCategoryName = string.Empty;
        public string ParentCategoryName
        {
            get => _parentCategoryName;
            set
            {
                _parentCategoryName = value;
                OnPropertyChanged(nameof(ParentCategoryName));
            }
        }

        private bool _isCategoryActive = true;
        public bool IsCategoryActive
        {
            get => _isCategoryActive;
            set
            {
                _isCategoryActive = value;
                OnPropertyChanged(nameof(IsCategoryActive));
            }
        }

        private int _displayOrder;
        public int DisplayOrder
        {
            get => _displayOrder;
            set
            {
                _displayOrder = value;
                OnPropertyChanged(nameof(DisplayOrder));
            }
        }

        /// <summary>0 = no parent. Bound to ComboBox; when saving we send null when 0.</summary>
        public int ParentCategoryId { get; set; }
        public string? Code { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }

        private string? _description;
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        // ---------------- COMMANDS ----------------

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public CategoryFormView() : this(null) { }

        public CategoryFormView(CategoryDto? dto)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                // Get CategoryApiService from DI container
                if (App.ServiceProvider != null)
                {
                    _service = (CategoryApiService)App.ServiceProvider.GetService(typeof(CategoryApiService))!;
                }
                else
                {
                    throw new InvalidOperationException("Application service provider not initialized.");
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize: {ex.Message}");
            }

            _editDto = dto;
            _isEdit = dto != null;

            Loaded += ProductFormView_Loaded;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isSaving);

            CancelCommand = new RelayCommand(CloseWindow);
            ResetCommand = new RelayCommand(ResetForm);

            // Prefill for AddSub/Edit
            if (_editDto != null)
            {
                // CategoryId is not directly editable in the form, it's derived from the selected parent
                // ParentCategoryId is bound to the ComboBox
                ParentCategoryId = _editDto.ParentCategoryId ?? 0;
                ParentCategoryName = _editDto.ParentCategoryName ?? string.Empty;
                CategoryName = _editDto.Name ?? string.Empty;
                IsCategoryActive = _editDto.IsActive;
                DisplayOrder = _editDto.DisplayOrder;
                Code = _editDto.Code;
                Slug = _editDto.Slug;
                Description = _editDto.Description;
            }

            // Force initial validation after prefill (so CategoryId error is not added for Add Root/Add Sub)
            ValidateAll();
        }

        // ---------------- LOAD ----------------

        private async void ProductFormView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
            OnPropertyChanged(nameof(CategoryName));
            OnPropertyChanged(nameof(IsCategoryActive));
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                Categories.Clear();
                Categories.Add(new LookupDto { Id = 0, Name = "-- No Parent --" });
                var list = await _service.GetAllAsync();
                foreach (var c in list)
                    Categories.Add(new LookupDto { Id = c.CategoryId, Name = c.IndentedName });
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Failed to load categories", ex.Message);
            }
        }

        // ---------------- SAVE ----------------

        private async Task SaveAsync()
        {
            ValidateAll();
            if (HasErrors)
            {
                var messages = _errors.SelectMany(kv => kv.Value).ToList();
                var text = messages.Count > 0 ? string.Join("\n", messages) : "Please fix validation errors.";
                FocusFirstInvalidControl();
                POS.UI.Components.DialogService.Info("Validation", text);
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new CategoryDto
            {
                CategoryId = _editDto?.CategoryId ?? 0, // Use existing CategoryId if editing, otherwise 0
                Name = CategoryName ?? string.Empty,
                ParentCategoryId = ParentCategoryId <= 0 ? null : ParentCategoryId,
                IsActive = IsCategoryActive,
                DisplayOrder = DisplayOrder,
                Code = Code,
                Slug = Slug,
                Description = Description,
                CreatedAt = DateTime.Now
            };

            try
            {
                var exists = await _service.CheckNameExistsAsync(dto.Name, dto.ParentCategoryId, _editDto?.CategoryId);
                if (exists)
                {
                    AddError(nameof(CategoryName), "Category name already exists under selected parent");
                    FocusFirstInvalidControl();
                    return;
                }
                if (_isEdit)
                {
                    dto.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(dto);
                }
                else
                {
                    await _service.AddAsync(dto);
                }

                DialogResult = true;
                CloseWindow();

            }
            catch (ApiValidationException vex)
            {
                if (vex.Error?.Errors != null)
                {
                    foreach (var kv in vex.Error.Errors)
                    {
                        ClearErrors(kv.Key);

                        foreach (var msg in kv.Value)
                            AddError(kv.Key, msg);
                    }

                    return; // 🔥 Inline errors shown, no MessageBox
                }
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Save Failed", ex.Message);
            }
            finally
            {
                _isSaving = false;
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
        private void CloseWindow() => Close();

        private void ResetForm()
        {
            CategoryName = string.Empty;
            ParentCategoryName = string.Empty;
            ParentCategoryId = 0;
            OnPropertyChanged(nameof(ParentCategoryId));
            Code = null;
            Slug = null;
            Description = null;
            ImageUrl = null;
            DisplayOrder = 0;
            IsCategoryActive = true;
        }

        private void ValidateAll()
        {
            CategoryName = CategoryName;
        }
        public class ApiValidationError
        {
            public Dictionary<string, string[]> Errors { get; set; }
        }

        



    }
}
