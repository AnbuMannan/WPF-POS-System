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

namespace POS.UI.Modules.Admin.TaxProfiles
{
    public partial class TaxProfileFormView : Window, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly TaxProfileApiService _service;
        private bool _isEdit;
        private TaxProfileDto _editDto;
        private bool _isSaving;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> _errors = new();
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        public System.Collections.IEnumerable GetErrors(string propertyName)
            => string.IsNullOrEmpty(propertyName) ? null : (_errors.ContainsKey(propertyName) ? _errors[propertyName] : null);

        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set
            {
                _profileName = value;
                OnPropertyChanged(nameof(ProfileName));
                ClearErrors(nameof(ProfileName));
                if (string.IsNullOrWhiteSpace(ProfileName))
                    AddError(nameof(ProfileName), "Tax profile name is required");
            }
        }

        private decimal _cgst;
        public decimal CGST
        {
            get => _cgst;
            set { _cgst = value; OnPropertyChanged(nameof(CGST)); }
        }

        private decimal _sgst;
        public decimal SGST
        {
            get => _sgst;
            set { _sgst = value; OnPropertyChanged(nameof(SGST)); }
        }

        private decimal _igst;
        public decimal IGST
        {
            get => _igst;
            set { _igst = value; OnPropertyChanged(nameof(IGST)); }
        }

        private decimal _cess;
        public decimal Cess
        {
            get => _cess;
            set { _cess = value; OnPropertyChanged(nameof(Cess)); }
        }

        private bool _isActiveChecked = true;
        /// <summary>Entity active flag. Named to avoid conflict with Window.IsActive (read-only).</summary>
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set { _isActiveChecked = value; OnPropertyChanged(nameof(IsActiveChecked)); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }

        public TaxProfileFormView() : this(null) { }

        public TaxProfileFormView(TaxProfileDto dto)
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                _service = App.ServiceProvider?.GetService(typeof(TaxProfileApiService)) as TaxProfileApiService
                    ?? throw new InvalidOperationException("Application service provider not initialized.");
            }
            catch (Exception ex)
            {
                POS.UI.Components.DialogService.Error("Initialization Error", ex.Message);
            }

            _editDto = dto;
            _isEdit = dto != null;

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !_isSaving && !HasErrors);
            CancelCommand = new RelayCommand(() => Close());
            ResetCommand = new RelayCommand(ResetForm);

            ValidateAll();

            if (_editDto != null)
            {
                ProfileName = _editDto.Name;
                CGST = _editDto.CGST;
                SGST = _editDto.SGST;
                IGST = _editDto.IGST;
                Cess = _editDto.Cess;
                IsActiveChecked = _editDto.IsActive;
            }
        }

        private async Task SaveAsync()
        {
            ValidateAll();
            if (HasErrors)
            {
                var messages = _errors.SelectMany(kv => kv.Value).ToList();
                var text = messages.Count > 0 ? string.Join("\n", messages) : "Please fix validation errors.";
                POS.UI.Components.DialogService.Info("Validation", text);
                return;
            }

            _isSaving = true;
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();

            var dto = new TaxProfileDto
            {
                TaxProfileId = _isEdit && _editDto != null ? _editDto.TaxProfileId : 0,
                Name = ProfileName ?? string.Empty,
                CGST = CGST,
                SGST = SGST,
                IGST = IGST,
                Cess = Cess,
                IsActive = IsActiveChecked,
                CreatedAt = _isEdit && _editDto != null ? _editDto.CreatedAt : DateTime.Now
            };

            try
            {
                if (_isEdit && _editDto != null)
                {
                    dto.UpdatedAt = DateTime.Now;
                    await _service.UpdateAsync(_editDto.TaxProfileId, dto);
                }
                else
                {
                    await _service.AddAsync(dto);
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

        private void ResetForm()
        {
            if (_editDto != null)
            {
                ProfileName = _editDto.Name;
                CGST = _editDto.CGST;
                SGST = _editDto.SGST;
                IGST = _editDto.IGST;
                Cess = _editDto.Cess;
                IsActiveChecked = _editDto.IsActive;
            }
            else
            {
                ProfileName = string.Empty;
                CGST = 0;
                SGST = 0;
                IGST = 0;
                Cess = 0;
                IsActiveChecked = true;
            }
        }

        private void ValidateAll()
        {
            if (string.IsNullOrWhiteSpace(ProfileName))
                AddError(nameof(ProfileName), "Tax profile name is required");
        }
    }
}
