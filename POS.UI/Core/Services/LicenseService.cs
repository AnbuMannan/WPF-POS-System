using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services
{
    /// <summary>
    /// License validation service for verifying application license.
    /// Communicates with external license validation API.
    /// </summary>
    public class LicenseService
    {
        public const string HttpClientName = "License";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private LicenseValidationResponse? _currentLicense;

        public LicenseService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = Log.ForContext<LicenseService>();
        }

        /// <summary>
        /// Gets the current license validation result.
        /// </summary>
        public LicenseValidationResponse? CurrentLicense => _currentLicense;

        /// <summary>
        /// Gets whether the application has a valid license.
        /// </summary>
        public bool IsLicenseValid => _currentLicense?.IsValid == true;

        /// <summary>
        /// Gets the license expiry date if valid.
        /// </summary>
        public DateTime? LicenseExpiryDate => _currentLicense?.ExpiryDate;

        /// <summary>
        /// Gets the license type.
        /// </summary>
        public string? LicenseType => _currentLicense?.LicenseType;

        /// <summary>
        /// Gets the days remaining until license expiry.
        /// </summary>
        public int DaysRemainingUntilExpiry
        {
            get
            {
                if (_currentLicense?.ExpiryDate == null)
                    return 0;

                var daysRemaining = (_currentLicense.ExpiryDate.Value - DateTime.Now).Days;
                return Math.Max(0, daysRemaining);
            }
        }

        /// <summary>
        /// Validates the license key for this device.
        /// </summary>
        public async Task<LicenseValidationResponse> ValidateLicenseAsync(string licenseKey, string? deviceId = null)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                throw new ArgumentException("License key cannot be empty", nameof(licenseKey));

            try
            {
                deviceId ??= GetDeviceId();

                _logger.Information("Validating license for device: {DeviceId}", deviceId);

                var request = new LicenseValidationRequest
                {
                    LicenseKey = licenseKey,
                    DeviceId = deviceId
                };

                var http = _httpClientFactory.CreateClient(HttpClientName);
                var response = await http.PostAsJsonAsync("api/license/validate", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.Warning("License validation failed: {StatusCode} - {Error}",
                        (int)response.StatusCode, errorContent);

                    return new LicenseValidationResponse
                    {
                        IsValid = false,
                        Message = "License validation failed"
                    };
                }

                var validationResponse = await response.Content.ReadFromJsonAsync<LicenseValidationResponse>();

                if (validationResponse is not null) // Use pattern matching for null check
                {
                    _currentLicense = validationResponse;

                    if (validationResponse.IsValid)
                    {
                        _logger.Information("License validated successfully. Type: {LicenseType}, Expires: {ExpiryDate}",
                            validationResponse.LicenseType, validationResponse.ExpiryDate);
                    }
                    else
                    {
                        _logger.Warning("License validation failed: {Message}", validationResponse.Message);
                    }

                    return validationResponse;
                }

                _logger.Warning("Invalid license validation response");
                return new LicenseValidationResponse
                {
                    IsValid = false,
                    Message = "Invalid response from license service"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "License validation HTTP request failed");
                throw new HttpRequestException("Failed to connect to license validation service", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "License validation error");
                throw;
            }
        }

        /// <summary>
        /// Checks if the license is expiring soon (within specified days).
        /// </summary>
        public bool IsLicenseExpiringSoon(int daysThreshold = 7)
        {
            if (!IsLicenseValid)
                return false;

            return DaysRemainingUntilExpiry <= daysThreshold && DaysRemainingUntilExpiry > 0;
        }

        /// <summary>
        /// Gets a unique device identifier.
        /// </summary>
        private string GetDeviceId()
        {
            try
            {
                // Use MAC address or Windows machine name + processor ID
                // For demo, using machine name (should use more unique identifier in production)
                var deviceId = Environment.MachineName;
                _logger.Debug("Generated device ID: {DeviceId}", deviceId);
                return deviceId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to generate device ID");
                return "UNKNOWN";
            }
        }
    }
}
