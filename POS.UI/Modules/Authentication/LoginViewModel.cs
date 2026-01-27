using POS.UI.Core.MVVM;
using POS.UI.Core.Services;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace POS.UI.Modules.Authentication
{
    /// <summary>
    /// ViewModel for login screen.
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthenticationService _authService;
        private readonly LicenseService _licenseService;
        private readonly ILogger _logger;

        private string? _username;
        private string? _password;
        private string? _statusMessage;
        private bool _isLoading;
        private bool _isLoginFailed;
        private RelayCommand? _loginCommand;
        private RelayCommand? _clearCommand;

        public LoginViewModel(AuthenticationService authService, LicenseService licenseService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
            _logger = Log.ForContext<LoginViewModel>();
        }

        /// <summary>
        /// Gets or sets the username input.
        /// </summary>
        public string? Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    UpdateLoginCommandCanExecute();
                }
            }
        }

        /// <summary>
        /// Gets or sets the password input.
        /// </summary>
        public string? Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    UpdateLoginCommandCanExecute();
                }
            }
        }

        /// <summary>
        /// Gets or sets the status message displayed to user.
        /// </summary>
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the loading state.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    UpdateLoginCommandCanExecute();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether login failed.
        /// </summary>
        public bool IsLoginFailed
        {
            get => _isLoginFailed;
            set
            {
                if (_isLoginFailed != value)
                {
                    _isLoginFailed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the login command.
        /// </summary>
        public ICommand LoginCommand =>
            _loginCommand ??= new RelayCommand(
                async () => await ExecuteLoginAsync(),
                () => CanLogin
            );

        /// <summary>
        /// Gets the clear command.
        /// </summary>
        public ICommand ClearCommand =>
            _clearCommand ??= new RelayCommand(ExecuteClear);

        /// <summary>
        /// Event raised when login succeeds.
        /// </summary>
        public event EventHandler? LoginSucceeded;

        /// <summary>
        /// Determines if login command can execute.
        /// </summary>
        private bool CanLogin => !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        /// Updates login command can execute state.
        /// </summary>
        private void UpdateLoginCommandCanExecute()
        {
            if (_loginCommand != null)
            {
                _loginCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Executes the login command asynchronously.
        /// </summary>
        private async Task ExecuteLoginAsync()
        {
            if (!CanLogin)
                return;

            IsLoading = true;
            IsLoginFailed = false;
            StatusMessage = "Logging in...";

            try
            {
                _logger.Information("Login attempt for user: {Username}", Username);

                //var loginResponse = await _authService.LoginAsync(Username!, Password!);

                //if (loginResponse.Success && _authService.IsAuthenticated)
                {
                    _logger.Information("Login successful for user: {Username}", Username);
                    StatusMessage = "Login successful!";
                    
                    // Validate license after successful login
                   // await ValidateLicenseAsync();

                    // Raise event to navigate to main window
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                //else
                //{
                //    _logger.Warning("Login failed: {Message}", loginResponse.Message);
                //    IsLoginFailed = true;
                //    StatusMessage = loginResponse.Message ?? "Login failed. Please check your credentials.";
                //}
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Login error");
                IsLoginFailed = true;
                StatusMessage = "An error occurred during login. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Validates the license for the authenticated user.
        /// </summary>
        private async Task ValidateLicenseAsync()
        {
            try
            {
                _logger.Debug("Starting license validation");
                StatusMessage = "Validating license...";

                // In a real implementation, you might get license key from config or user input
                const string licenseKey = "YOUR-LICENSE-KEY-HERE";  // Get from config

                var licenseResponse = await _licenseService.ValidateLicenseAsync(licenseKey);

                if (!licenseResponse.IsValid)
                {
                    _logger.Error("License validation failed: {Message}", licenseResponse.Message);
                    StatusMessage = $"License Error: {licenseResponse.Message}";
                    IsLoginFailed = true;
                    
                    // Clear authentication if license invalid
                    await _authService.LogoutAsync();
                    return;
                }

                _logger.Information("License validated. Type: {LicenseType}, Expires: {ExpiryDate}",
                    licenseResponse.LicenseType, licenseResponse.ExpiryDate);

                // Check if license expiring soon
                if (_licenseService.IsLicenseExpiringSoon(7))
                {
                    _logger.Warning("License expiring soon in {DaysRemaining} days",
                        _licenseService.DaysRemainingUntilExpiry);
                    StatusMessage = $"Note: Your license expires in {_licenseService.DaysRemainingUntilExpiry} days.";
                }
                else
                {
                    StatusMessage = "License validated successfully.";
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "License validation error");
                // Don't fail login if license validation fails (soft error)
                StatusMessage = "Warning: Could not validate license.";
            }
        }

        // Add inside LoginViewModel class
        public Action? ClearPasswordAction { get; set; }


        // Modify ExecuteClear() like this:
        private void ExecuteClear()
        {
            Username = null;
            Password = null;
            StatusMessage = null;
            IsLoginFailed = false;


            // Clear PasswordBox from View
            ClearPasswordAction?.Invoke();
        }
    }
}
