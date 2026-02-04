using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Admin.Customers
{
    public partial class CustomerFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly CustomerApiService? _service;
        private bool _isEdit;
        private CustomerDto? _editDto;
        private bool _isSaving;
        private bool _isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> _errors = new();
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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

        public System.Collections.IEnumerable? GetErrors(string propertyName)
            => string.IsNullOrEmpty(propertyName) ? null : (_errors.ContainsKey(propertyName) ? _errors[propertyName] : null);

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value ?? string.Empty;
                OnPropertyChanged(nameof(CustomerName));
                ClearErrors(nameof(CustomerName));
                if (string.IsNullOrWhiteSpace(CustomerName))
                    AddError(nameof(CustomerName), "Customer name is required");
            }
        }

        private string? _phone;
        public string? Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
                if (!_isLoading)
                    _ = ValidatePhoneAsync();
            }
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string? _address;
        public string? Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(nameof(Address)); }
        }

        private int _loyaltyPoints;
        public int LoyaltyPoints
        {
            get => _loyaltyPoints;
            set { _loyaltyPoints = value; OnPropertyChanged(nameof(LoyaltyPoints)); }
        }

        private bool _isActiveChecked = true;
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set { _isActiveChecked = value; OnPropertyChanged(nameof(IsActiveChecked)); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public CustomerFormView() : this(null) { }

        public CustomerFormView(CustomerDto? dto)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                _service = App.ServiceProvider?.GetService(typeof(CustomerApiService)) as CustomerApiService
                    ?? throw new InvalidOperationException("Application service provider not initialized.");
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", $"Failed to initialize: {ex.Message}");
            }

            _editDto = dto;
            _isEdit = dto != null;

            SaveCommand = new RelayCommand(ExecuteSave, () => !_isSaving);
            CancelCommand = new RelayCommand(CloseWindow);
            ResetCommand = new RelayCommand(ResetForm);

            _isLoading = true;
            if (_editDto != null)
            {
                CustomerName = _editDto.Name;
                Phone = _editDto.Phone ?? string.Empty;
                Email = _editDto.Email ?? string.Empty;
                Address = _editDto.Address ?? string.Empty;
                LoyaltyPoints = _editDto.LoyaltyPoints;
                IsActiveChecked = _editDto.IsActive;
            }
            _isLoading = false;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            Loaded += (s, e) => ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private async void ExecuteSave()
        {
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Save Failed", ex.Message);
            }
        }

        private async Task SaveAsync()
        {
            if (_service == null)
            {
                POS.UI.Components.DialogService.Error("Save Failed", "Customer service is not available.");
                return;
            }

            ClearErrors(nameof(CustomerName));
            ClearErrors(nameof(Phone));
            if (string.IsNullOrWhiteSpace(CustomerName))
                AddError(nameof(CustomerName), "Customer name is required");
            if (HasErrors)
            {
                var messages = _errors.SelectMany(kv => kv.Value).ToList();
                POS.UI.Components.DialogService.Info("Validation", messages.Count > 0 ? string.Join("\n", messages) : "Please fix validation errors.");
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new CustomerDto
            {
                Id = _isEdit && _editDto != null ? _editDto.Id : Guid.NewGuid(),
                Name = CustomerName?.Trim() ?? string.Empty,
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                LoyaltyPoints = LoyaltyPoints,
                IsActive = IsActiveChecked,
                CreatedAt = _isEdit && _editDto != null ? _editDto.CreatedAt : DateTime.UtcNow,
                UpdatedAt = _isEdit ? DateTime.UtcNow : null
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(Phone))
                {
                    var exists = await _service.CheckPhoneExistsAsync(Phone.Trim(), _isEdit && _editDto != null ? _editDto.Id : null);
                    if (exists)
                    {
                        AddError(nameof(Phone), "Phone number already exists");
                        _isSaving = false;
                        ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                        POS.UI.Components.DialogService.Info("Validation", "This phone number is already in use by another customer.");
                        return;
                    }
                }

                if (_isEdit && _editDto != null)
                {
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
                var exists = await _service!.CheckPhoneExistsAsync(Phone.Trim(), _isEdit && _editDto != null ? _editDto.Id : null);
                if (exists)
                    AddError(nameof(Phone), "Phone number already exists");
            }
            catch
            {
                // Ignore API failures for live validation
            }
        }

        private void CloseWindow() => Close();

        private void ResetForm()
        {
            CustomerName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            LoyaltyPoints = 0;
            IsActiveChecked = true;
        }
    }
}
