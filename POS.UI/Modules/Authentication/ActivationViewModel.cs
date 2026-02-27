using System;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.UI.Components;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using POS.Shared.Models;

namespace POS.UI.Modules.Authentication
{
    public class ActivationViewModel : ViewModelBase
    {
        private readonly AuthenticationService _authService;
        private readonly StoreApiService _storeApi;
        private readonly LocalSettingsService _localSettings;

        private string _licenseKey = string.Empty;
        private bool _isBusy;
        private string _statusMessage = string.Empty;

        public ActivationViewModel(
            AuthenticationService authService, 
            StoreApiService storeApi, 
            LocalSettingsService localSettings)
        {
            _authService = authService;
            _storeApi = storeApi;
            _localSettings = localSettings;

            ActivateCommand = new RelayCommand(async () => await ExecuteActivateAsync(), () => !string.IsNullOrWhiteSpace(LicenseKey) && !IsBusy);
        }

        public string LicenseKey
        {
            get => _licenseKey;
            set { _licenseKey = value; OnPropertyChanged(); ((RelayCommand)ActivateCommand).RaiseCanExecuteChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); ((RelayCommand)ActivateCommand).RaiseCanExecuteChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand ActivateCommand { get; }

        public event Action? ActivationSuccess;

        private async Task ExecuteActivateAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Activating terminal...";

                // 1. Activate via Auth Server
                var result = await _authService.ActivateLicenseAsync(LicenseKey);
                if (!result.IsValid || result.Store == null)
                {
                    StatusMessage = result.Message ?? "Activation failed.";
                    DialogService.Error("Activation Error", StatusMessage);
                    return;
                }

                // 2. Sync Store to Core DB
                StatusMessage = "Syncing store configuration...";
                await _storeApi.SyncStoreAsync(result.Store);

                // 3. Save to Local Settings
                var config = _localSettings.GetConfig();
                config.StoreCode = result.Store.StoreCode;
                config.StoreName = result.Store.StoreName;
                _localSettings.SaveConfig(config);

                StatusMessage = "Activation successful!";
                DialogService.Success("Success", "Terminal activated successfully. Please login to continue.");
                
                ActivationSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error: " + ex.Message;
                DialogService.Error("Critical Error", StatusMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}