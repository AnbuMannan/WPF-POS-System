using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using POS.Shared.Models;
using POS.UI.Core.MVVM;
using POS.UI.Core.Services;

namespace POS.UI.Modules.Settings
{
    public class SystemPreferencesViewModel : ViewModelBase
    {
        private readonly SystemPreferenceApiService _apiService;
        private readonly LocalSettingsService _localSettings;
        
        private SystemPreferenceDto? _preferences;
        private int _sidebarIdleTimeoutSeconds;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isStatusSuccess = true;

        public SystemPreferencesViewModel(SystemPreferenceApiService apiService, LocalSettingsService localSettings)
        {
            _apiService = apiService;
            _localSettings = localSettings;
            
            LoadPreferencesCommand = new RelayCommand(async () => await LoadPreferencesAsync());
            SavePreferencesCommand = new RelayCommand(async () => await SavePreferencesAsync());
            
            // Load preferences when view model is created
            LoadPreferencesAsync().ConfigureAwait(false);
        }

        public SystemPreferenceDto? Preferences
        {
            get => _preferences;
            set { _preferences = value; OnPropertyChanged(); }
        }

        public int SidebarIdleTimeoutSeconds
        {
            get => _sidebarIdleTimeoutSeconds;
            set { _sidebarIdleTimeoutSeconds = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsStatusSuccess
        {
            get => _isStatusSuccess;
            set { _isStatusSuccess = value; OnPropertyChanged(); }
        }

        public ICommand LoadPreferencesCommand { get; }
        public ICommand SavePreferencesCommand { get; }

        private async Task LoadPreferencesAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading system preferences...";
            IsStatusSuccess = true;

            try
            {
                var config = _localSettings.GetConfig();
                var storeCode = config.StoreCode;
                
                Preferences = await _apiService.GetByStoreAsync(storeCode);
                
                if (Preferences != null)
                {
                    SidebarIdleTimeoutSeconds = Preferences.SidebarIdleTimeoutSeconds;
                    StatusMessage = "Preferences loaded successfully";
                }
                else
                {
                    // Use default values if no preferences exist
                    SidebarIdleTimeoutSeconds = 10;
                    StatusMessage = "Using default preferences";
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Failed to load preferences: {ex.Message}";
                IsStatusSuccess = false;
                // Fall back to default values
                SidebarIdleTimeoutSeconds = 10;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SavePreferencesAsync()
        {
            IsLoading = true;
            StatusMessage = "Saving preferences...";
            IsStatusSuccess = true;

            try
            {
                var config = _localSettings.GetConfig();
                var storeCode = config.StoreCode;
                
                var dto = new UpdateSystemPreferenceDto
                {
                    SidebarIdleTimeoutSeconds = SidebarIdleTimeoutSeconds
                };
                
                var result = await _apiService.UpdateAsync(storeCode, dto);
                
                if (result.Success)
                {
                    Preferences = result.Preferences;
                    StatusMessage = "Preferences saved successfully";
                }
                else
                {
                    StatusMessage = $"Failed to save preferences: {result.Message}";
                    IsStatusSuccess = false;
                }
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"Failed to save preferences: {ex.Message}";
                IsStatusSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}