using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using POS.Shared.Models;
using Serilog;

namespace POS.UI.Core.Services
{
    public class SystemPreferenceApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger = Log.ForContext<SystemPreferenceApiService>();

        public SystemPreferenceApiService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<SystemPreferenceDto?> GetByStoreAsync(int storeCode)
        {
            try
            {
                // Add X-Store-Code header for multi-tenant support
                var request = new HttpRequestMessage(HttpMethod.Get, "api/system-preferences");
                request.Headers.Add("X-Store-Code", storeCode.ToString());
                
                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                var preferences = await response.Content.ReadFromJsonAsync<SystemPreferenceDto>();
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get system preferences for store {StoreCode}", storeCode);
                return null;
            }
        }

        public async Task<(bool Success, string Message, SystemPreferenceDto? Preferences)> UpdateAsync(int storeCode, UpdateSystemPreferenceDto dto)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Add X-Store-Code header for multi-tenant support
                var request = new HttpRequestMessage(HttpMethod.Put, "api/system-preferences")
                {
                    Content = content
                };
                request.Headers.Add("X-Store-Code", storeCode.ToString());
                
                var response = await _http.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var preferences = await response.Content.ReadFromJsonAsync<SystemPreferenceDto>();
                    return (true, "System preferences updated successfully", preferences);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, error, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update system preferences for store {StoreCode}", storeCode);
                return (false, ex.Message, null);
            }
        }
    }
}