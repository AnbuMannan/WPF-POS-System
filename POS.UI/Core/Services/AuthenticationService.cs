using POS.UI.Core.Exceptions;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services
{
    /// <summary>
    /// DTO for login request.
    /// </summary>
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    /// <summary>
    /// DTO for login response containing JWT token.
    /// </summary>
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// User information returned from authentication.
    /// </summary>
    public class UserInfo
    {
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string[]? Roles { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for license validation request.
    /// </summary>
    public class LicenseValidationRequest
    {
        public string? LicenseKey { get; set; }
        public string? DeviceId { get; set; }
    }

    /// <summary>
    /// DTO for license validation response.
    /// </summary>
    public class LicenseValidationResponse
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? LicenseType { get; set; }
    }

    /// <summary>
    /// Authentication service for handling login, token refresh, and logout.
    /// Communicates with external authentication API.
    /// </summary>
    public class AuthenticationService
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger;
        private string? _currentToken;
        private string? _currentRefreshToken;
        private UserInfo? _currentUser;

        public AuthenticationService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = Log.ForContext<AuthenticationService>();
        }

        /// <summary>
        /// Gets the currently authenticated user.
        /// </summary>
        public UserInfo? CurrentUser => _currentUser;

        /// <summary>
        /// Gets the current authentication token.
        /// </summary>
        public string? CurrentToken => _currentToken;

        /// <summary>
        /// Gets whether a user is currently authenticated.
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken) && _currentUser != null;

        /// <summary>
        /// Logs in a user with username and password.
        /// </summary>
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            try
            {
                _logger.Information("Attempting login for user: {Username}", username);

                var request = new LoginRequest { Username = username, Password = password };
                var response = await _http.PostAsJsonAsync("api/auth/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.Warning("Login failed for user {Username}: {StatusCode} - {Error}",
                        username, (int)response.StatusCode, errorContent);
                    
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Login failed. Please check your credentials."
                    };
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    _currentToken = loginResponse.Token;
                    _currentRefreshToken = loginResponse.RefreshToken;
                    _currentUser = loginResponse.User;

                    _logger.Information("User {Username} logged in successfully. Token expires in {ExpiryMinutes} minutes",
                        username, 60); // Adjust based on your token expiry

                    return loginResponse;
                }

                _logger.Warning("Login response invalid for user {Username}", username);
                return loginResponse ?? new LoginResponse 
                { 
                    Success = false, 
                    Message = "Invalid login response" 
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Login HTTP request failed for user {Username}", username);
                throw new HttpRequestException("Failed to connect to authentication service", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Login error for user {Username}", username);
                throw;
            }
        }

        /// <summary>
        /// Refreshes the authentication token using the refresh token.
        /// </summary>
        public async Task<bool> RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(_currentRefreshToken))
            {
                _logger.Warning("Cannot refresh token: no refresh token available");
                return false;
            }

            try
            {
                _logger.Debug("Refreshing authentication token");

                var request = new { RefreshToken = _currentRefreshToken };
                var response = await _http.PostAsJsonAsync("api/auth/refresh", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Warning("Token refresh failed: {StatusCode}", (int)response.StatusCode);
                    ClearSession();
                    return false;
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    _currentToken = loginResponse.Token;
                    _currentRefreshToken = loginResponse.RefreshToken;
                    _currentUser = loginResponse.User;

                    _logger.Information("Token refreshed successfully");
                    return true;
                }

                _logger.Warning("Token refresh response invalid");
                ClearSession();
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Token refresh failed");
                ClearSession();
                return false;
            }
        }

        /// <summary>
        /// Logs out the current user and clears the session.
        /// </summary>
        public async Task LogoutAsync()
        {
            if (!IsAuthenticated)
            {
                _logger.Debug("Logout called but no user authenticated");
                return;
            }

            try
            {
                _logger.Information("Logging out user: {Username}", _currentUser?.Username);

                var request = new { Token = _currentToken };
                var response = await _http.PostAsJsonAsync("api/auth/logout", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Warning("Logout API call failed: {StatusCode}", (int)response.StatusCode);
                }

                ClearSession();
                _logger.Information("User {Username} logged out", _currentUser?.Username);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Logout error");
                // Clear session anyway
                ClearSession();
            }
        }

        /// <summary>
        /// Checks if the current user has a specific role.
        /// </summary>
        public bool HasRole(string role)
        {
            if (_currentUser?.Roles == null)
                return false;

            return Array.Exists(_currentUser.Roles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Checks if the current user has any of the specified roles.
        /// </summary>
        public bool HasAnyRole(params string[] roles)
        {
            if (_currentUser?.Roles == null || roles.Length == 0)
                return false;

            foreach (var role in roles)
            {
                if (Array.Exists(_currentUser.Roles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the current session (token, refresh token, user info).
        /// </summary>
        private void ClearSession()
        {
            _currentToken = null;
            _currentRefreshToken = null;
            _currentUser = null;
        }
    }
}
