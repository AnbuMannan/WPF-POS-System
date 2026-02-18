using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using POS.Shared.Models;
using Serilog;

namespace POS.UI.Core.Services
{
    public class CompanyProfileApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger = Log.ForContext<CompanyProfileApiService>();

        public CompanyProfileApiService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<CompanyProfileDto?> GetAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/company-profile");
                response.EnsureSuccessStatusCode();
                var profile = await response.Content.ReadFromJsonAsync<CompanyProfileDto>();
                if (profile != null)
                {
                    profile.LogoUrl = BuildFullUrl(profile.LogoUrl);
                }
                return profile;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get company profile");
                return null;
            }
        }

        public async Task<(bool Success, string Message, CompanyProfileDto? Profile)> SaveAsync(UpdateCompanyProfileDto dto)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PutAsync("api/company-profile", content);

                if (response.IsSuccessStatusCode)
                {
                    var profile = await response.Content.ReadFromJsonAsync<CompanyProfileDto>();
                    return (true, "Company profile saved successfully", profile);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, error, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save company profile");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? LogoUrl, string Message)> UploadLogoAsync(string filePath)
        {
            try
            {
                using var fileStream = File.OpenRead(filePath);
                using var content = new MultipartFormDataContent();
                var fileName = Path.GetFileName(filePath);
                content.Add(new StreamContent(fileStream), "file", fileName);

                var response = await _http.PostAsync("api/company-profile/logo", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LogoUploadResponse>();
                    var fullUrl = BuildFullUrl(result?.LogoUrl);
                    return (true, fullUrl, "Logo uploaded successfully");
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, null, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to upload logo");
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Converts a relative URL (e.g. /uploads/logo/file.png) to a full URL using the API base address.
        /// WPF Image controls require a full URL to display images from the server.
        /// </summary>
        private string? BuildFullUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return null;
            if (relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return relativeUrl;
            if (_http.BaseAddress != null)
            {
                return new Uri(_http.BaseAddress, relativeUrl).ToString();
            }
            return relativeUrl;
        }

        private class LogoUploadResponse
        {
            public string? LogoUrl { get; set; }
        }
    }
}
