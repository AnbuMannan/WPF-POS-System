using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Suppliers.SupplierList
{
    public partial class SupplierFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly SupplierApiService _service;
        private bool _isEdit;
        private SupplierDto? _editDto;
        private bool _isSaving;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ---------------- VALIDATION ENGINE ----------------

        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> _errors = new();

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void FocusFirstInvalidControl()
        {
            var firstInvalid = FindFirstInvalid(this);
            if (firstInvalid != null)
            {
                firstInvalid.BringIntoView();
                firstInvalid.Focus();
            }
        }

        private System.Windows.Controls.Control? FindFirstInvalid(System.Windows.DependencyObject parent)
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                if (child is System.Windows.Controls.Control control &&
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
                _errors[propertyName] = new System.Collections.Generic.List<string>();

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

        // ---------------- FIELDS ----------------

        private string _supplierName = string.Empty;
        public string SupplierName
        {
            get => _supplierName;
            set
            {
                _supplierName = value;
                OnPropertyChanged(nameof(SupplierName));
                ClearErrors(nameof(SupplierName));
                if (string.IsNullOrWhiteSpace(SupplierName))
                    AddError(nameof(SupplierName), "Supplier name is required");
            }
        }

        private string _supplierCode = string.Empty;
        public string SupplierCode
        {
            get => _supplierCode;
            set
            {
                _supplierCode = value;
                OnPropertyChanged(nameof(SupplierCode));
                ClearErrors(nameof(SupplierCode));
                if (string.IsNullOrWhiteSpace(SupplierCode))
                    AddError(nameof(SupplierCode), "Supplier code is required");
                else
                    _ = ValidateCodeAsync();
            }
        }

        private string _contactPerson = string.Empty;
        public string ContactPerson
        {
            get => _contactPerson;
            set
            {
                _contactPerson = value;
                OnPropertyChanged(nameof(ContactPerson));
            }
        }

        private string _mobile = string.Empty;
        public string Mobile
        {
            get => _mobile;
            set
            {
                _mobile = value;
                OnPropertyChanged(nameof(Mobile));
                ClearErrors(nameof(Mobile));
                if (!string.IsNullOrWhiteSpace(Mobile) && !IsValidMobile(Mobile))
                    AddError(nameof(Mobile), "Invalid mobile number format");
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                ClearErrors(nameof(Email));
                if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
                    AddError(nameof(Email), "Invalid email format");
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private string _gstVatNumber = string.Empty;
        public string GstVatNumber
        {
            get => _gstVatNumber;
            set
            {
                _gstVatNumber = value;
                OnPropertyChanged(nameof(GstVatNumber));
            }
        }

        private string _creditPeriodDays = string.Empty;
        public string CreditPeriodDays
        {
            get => _creditPeriodDays;
            set
            {
                _creditPeriodDays = value;
                OnPropertyChanged(nameof(CreditPeriodDays));
            }
        }

        private string _creditLimit = string.Empty;
        public string CreditLimit
        {
            get => _creditLimit;
            set
            {
                _creditLimit = value;
                OnPropertyChanged(nameof(CreditLimit));
            }
        }

        private bool _isActiveChecked = true;
        /// <summary>Entity active flag. Named to avoid conflict with Window.IsActive (read-only).</summary>
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set
            {
                _isActiveChecked = value;
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        // ---------------- COMMANDS ----------------

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public SupplierFormView() : this(null) { }

        public SupplierFormView(SupplierDto dto)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                if (App.ServiceProvider != null)
                {
                    _service = (SupplierApiService)App.ServiceProvider.GetService(typeof(SupplierApiService));
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

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isSaving && !HasErrors);
            CancelCommand = new RelayCommand(CloseWindow);
            ResetCommand = new RelayCommand(ResetForm);

            ValidateAll();

            if (_editDto != null)
            {
                SupplierName = _editDto.Name;
                SupplierCode = _editDto.Code;
                ContactPerson = _editDto.ContactPerson;
                Mobile = _editDto.Mobile;
                Email = _editDto.Email;
                Address = _editDto.Address;
                GstVatNumber = _editDto.GstVatNumber;
                CreditPeriodDays = _editDto.CreditPeriodDays?.ToString();
                CreditLimit = _editDto.CreditLimit?.ToString("F2");
                IsActiveChecked = _editDto.IsActive;
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

            var dto = new SupplierDto
            {
                Id = _isEdit && _editDto != null ? _editDto.Id : Guid.Empty,
                Name = SupplierName ?? string.Empty,
                Code = SupplierCode ?? string.Empty,
                ContactPerson = ContactPerson,
                Mobile = Mobile,
                Email = Email,
                Address = Address,
                GstVatNumber = GstVatNumber,
                CreditPeriodDays = int.TryParse(CreditPeriodDays, out var days) ? days : (int?)null,
                CreditLimit = decimal.TryParse(CreditLimit, out var limit) ? limit : (decimal?)null,
                IsActive = IsActiveChecked,
                CreatedAt = _isEdit && _editDto != null ? _editDto.CreatedAt : DateTime.Now
            };

            try
            {
                var exists = await _service.CheckCodeExistsAsync(SupplierCode, _isEdit && _editDto != null ? _editDto.Id : (Guid?)null);
                if (exists)
                {
                    AddError(nameof(SupplierCode), "Supplier code already exists");
                    FocusFirstInvalidControl();
                    return;
                }

                if (_isEdit && _editDto != null)
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
                    return;
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

        private async Task ValidateCodeAsync()
        {
            ClearErrors(nameof(SupplierCode));
            if (string.IsNullOrWhiteSpace(SupplierCode))
                return;

            try
            {
                await Task.Delay(400);
                bool exists = await _service.CheckCodeExistsAsync(SupplierCode, _isEdit && _editDto != null ? _editDto.Id : (Guid?)null);
                if (exists)
                    AddError(nameof(SupplierCode), "Supplier code already exists");
            }
            catch
            {
                // Ignore API failures silently
            }
        }

        private void CloseWindow() => Close();

        private void ResetForm()
        {
            SupplierName = string.Empty;
            SupplierCode = string.Empty;
            ContactPerson = string.Empty;
            Mobile = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            GstVatNumber = string.Empty;
            CreditPeriodDays = string.Empty;
            CreditLimit = string.Empty;
            IsActiveChecked = true;
        }

        private void ValidateAll()
        {
            SupplierName = SupplierName ?? string.Empty;
            SupplierCode = SupplierCode ?? string.Empty;
        }

        // ---------------- VALIDATION HELPERS ----------------

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidMobile(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return false;
            var pattern = @"^[0-9\+\-\(\)\s]{7,20}$";
            return Regex.IsMatch(mobile, pattern);
        }
    }
}
