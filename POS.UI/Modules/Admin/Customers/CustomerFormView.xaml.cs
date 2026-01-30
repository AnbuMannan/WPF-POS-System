using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Admin.Customers
{
    public partial class CustomerFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly CustomerApiService _service;
        private bool _isEdit;
        private CustomerDto _editDto;
        private bool _isSaving;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ---------------- VALIDATION ENGINE ----------------

        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> _errors = new();

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void FocusFirstInvalidControl()
        {
            var firstInvalid = FindFirstInvalid(this);
            if (firstInvalid != null)
            {
                firstInvalid.BringIntoView();
                firstInvalid.Focus();
            }
        }

        private System.Windows.Controls.Control FindFirstInvalid(System.Windows.DependencyObject parent)
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

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                ClearErrors(nameof(FirstName));
                if (string.IsNullOrWhiteSpace(FirstName))
                    AddError(nameof(FirstName), "First name is required");
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                ClearErrors(nameof(LastName));
                if (string.IsNullOrWhiteSpace(LastName))
                    AddError(nameof(LastName), "Last name is required");
            }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
                _ = ValidatePhoneAsync();
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                _dateOfBirth = value;
                OnPropertyChanged(nameof(DateOfBirth));
            }
        }

        private string _loyaltyNumber;
        public string LoyaltyNumber
        {
            get => _loyaltyNumber;
            set
            {
                _loyaltyNumber = value;
                OnPropertyChanged(nameof(LoyaltyNumber));
            }
        }

        private bool _isWholesale;
        public bool IsWholesale
        {
            get => _isWholesale;
            set
            {
                _isWholesale = value;
                OnPropertyChanged(nameof(IsWholesale));
            }
        }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        // ---------------- COMMANDS ----------------

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public CustomerFormView() : this(null) { }

        public CustomerFormView(CustomerDto dto)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                if (App.ServiceProvider != null)
                {
                    _service = (CustomerApiService)App.ServiceProvider.GetService(typeof(CustomerApiService));
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
                FirstName = _editDto.FirstName;
                LastName = _editDto.LastName;
                Phone = _editDto.Phone;
                Email = _editDto.Email;
                Address = _editDto.Address;
                DateOfBirth = _editDto.DateOfBirth;
                LoyaltyNumber = _editDto.LoyaltyNumber;
                IsWholesale = _editDto.IsWholesale;
                IsActive = _editDto.IsActive;
            }
        }

        // ---------------- SAVE ----------------

        private async Task SaveAsync()
        {
            ValidateAll();
            if (HasErrors)
            {
                FocusFirstInvalidControl();
                POS.UI.Components.DialogService.Info("Validation", "Please fix highlighted validation errors.");
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new CustomerDto
            {
                CustomerId = _isEdit && _editDto != null ? _editDto.CustomerId : Guid.NewGuid().ToString(),
                FirstName = FirstName ?? string.Empty,
                LastName = LastName ?? string.Empty,
                Phone = Phone,
                Email = Email,
                Address = Address,
                DateOfBirth = DateOfBirth,
                LoyaltyNumber = LoyaltyNumber,
                IsWholesale = IsWholesale,
                IsActive = IsActive,
                CreatedAt = _isEdit && _editDto != null ? _editDto.CreatedAt : DateTime.Now
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(Phone))
                {
                    var exists = await _service.CheckPhoneExistsAsync(Phone, _isEdit && _editDto != null ? _editDto.CustomerId : null);
                    if (exists)
                    {
                        AddError(nameof(Phone), "Phone number already exists");
                        FocusFirstInvalidControl();
                        return;
                    }
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

        private async Task ValidatePhoneAsync()
        {
            ClearErrors(nameof(Phone));
            if (string.IsNullOrWhiteSpace(Phone))
                return;

            try
            {
                await Task.Delay(400);
                bool exists = await _service.CheckPhoneExistsAsync(Phone, _isEdit && _editDto != null ? _editDto.CustomerId : null);
                if (exists)
                    AddError(nameof(Phone), "Phone number already exists");
            }
            catch
            {
                // Ignore API failures silently
            }
        }

        private void CloseWindow() => Close();

        private void ResetForm()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            DateOfBirth = null;
            LoyaltyNumber = string.Empty;
            IsWholesale = false;
            IsActive = true;
        }

        private void ValidateAll()
        {
            FirstName = FirstName ?? string.Empty;
            LastName = LastName ?? string.Empty;
        }
    }
}
