using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace POS.UI.Modules.Admin.Uom
{
    public partial class UomFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly UomApiService _service;
        private readonly bool _isEdit;
        private readonly UomDto _editDto;
        private bool _isSaving;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> _errors = new();
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
            => string.IsNullOrEmpty(propertyName) ? null : (_errors.ContainsKey(propertyName) ? _errors[propertyName] : null);

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName)) _errors[propertyName] = new System.Collections.Generic.List<string>();
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

        private string _uomName;
        public string UomName
        {
            get => _uomName;
            set { _uomName = value; OnPropertyChanged(nameof(UomName)); ClearErrors(nameof(UomName)); if (string.IsNullOrWhiteSpace(_uomName)) AddError(nameof(UomName), "Name is required"); }
        }

        private string _code;
        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(nameof(Code)); ClearErrors(nameof(Code)); if (string.IsNullOrWhiteSpace(_code)) AddError(nameof(Code), "Code is required"); _ = ValidateCodeAsync(); }
        }

        private string _symbol;
        public string Symbol { get => _symbol; set { _symbol = value; OnPropertyChanged(nameof(Symbol)); } }

        private string _decimalPlacesText = "2";
        public string DecimalPlacesText
        {
            get => _decimalPlacesText;
            set { _decimalPlacesText = value; OnPropertyChanged(nameof(DecimalPlacesText)); ClearErrors(nameof(DecimalPlacesText)); ValidateDecimalPlaces(); }
        }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        private bool _isActiveChecked = true;
        public bool IsActiveChecked { get => _isActiveChecked; set { _isActiveChecked = value; OnPropertyChanged(nameof(IsActiveChecked)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public UomFormView() : this(null) { }

        public UomFormView(UomDto dto)
        {
            InitializeComponent();
            DataContext = this;
            _service = App.ServiceProvider?.GetService(typeof(UomApiService)) as UomApiService
                ?? throw new InvalidOperationException("UomApiService not registered.");
            _editDto = dto;
            _isEdit = dto != null;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isSaving && !HasErrors);
            CancelCommand = new RelayCommand(() => Close());
            ResetCommand = new RelayCommand(ResetForm);

            if (_editDto != null)
            {
                Name = _editDto.Name ?? string.Empty;
                Code = _editDto.Code ?? string.Empty;
                Symbol = _editDto.Symbol ?? string.Empty;
                DecimalPlacesText = _editDto.DecimalPlaces.ToString();
                Description = _editDto.Description ?? string.Empty;
                IsActiveChecked = _editDto.IsActive;
            }
        }

        private void ValidateDecimalPlaces()
        {
            if (string.IsNullOrWhiteSpace(DecimalPlacesText)) return;
            if (!int.TryParse(DecimalPlacesText, out int v) || v < 0 || v > 6)
                AddError(nameof(DecimalPlacesText), "Decimal places must be 0–6");
        }

        private async System.Threading.Tasks.Task ValidateCodeAsync()
        {
            ClearErrors(nameof(Code));
            if (string.IsNullOrWhiteSpace(Code)) return;
            try
            {
                await System.Threading.Tasks.Task.Delay(400);
                var excludeId = _isEdit && _editDto != null ? _editDto.Id : (Guid?)null;
                if (await _service.CodeExistsAsync(Code, excludeId))
                    AddError(nameof(Code), "Code already exists");
            }
            catch { }
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            ValidateDecimalPlaces();
            if (HasErrors)
            {
                var messages = _errors.SelectMany(kv => kv.Value).ToList();
                POS.UI.Components.DialogService.Info("Validation", messages.Count > 0 ? string.Join("\n", messages) : "Please fix validation errors.");
                return;
            }

            if (!int.TryParse(DecimalPlacesText, out int dec) || dec < 0 || dec > 6)
            {
                POS.UI.Components.DialogService.Info("Validation", "Decimal places must be 0–6.");
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new UomDto
            {
                Id = _isEdit && _editDto != null ? _editDto.Id : Guid.Empty,
                Name = UomName?.Trim() ?? string.Empty,
                Code = Code?.Trim() ?? string.Empty,
                Symbol = string.IsNullOrWhiteSpace(Symbol) ? null : Symbol.Trim(),
                DecimalPlaces = dec,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                IsActive = IsActiveChecked,
                CreatedAt = _isEdit && _editDto != null ? _editDto.CreatedAt : DateTime.Now
            };

            try
            {
                if (_isEdit && _editDto != null)
                {
                    dto.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(dto.Id, dto);
                }
                else
                {
                    await _service.CreateAsync(dto);
                }
                DialogResult = true;
                Close();
            }
            catch (ApiValidationException vex)
            {
                if (vex.Error?.Errors != null)
                {
                    foreach (var kv in vex.Error.Errors)
                    {
                        ClearErrors(kv.Key);
                        foreach (var msg in kv.Value) AddError(kv.Key, msg);
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

        private void ResetForm()
        {
            UomName = string.Empty;
            Code = string.Empty;
            Symbol = string.Empty;
            DecimalPlacesText = "2";
            Description = string.Empty;
            IsActiveChecked = true;
        }
    }
}
