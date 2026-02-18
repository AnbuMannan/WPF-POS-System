using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using POS.Shared.Models;
using Serilog;

namespace POS.UI.Core.Services
{
    public class CashTransactionApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger = Log.ForContext<CashTransactionApiService>();

        public CashTransactionApiService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<CashTransactionDto>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var url = "api/cash-transactions";
                var queryParams = new List<string>();
                
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                
                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);

                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<CashTransactionDto>>() ?? new List<CashTransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get cash transactions");
                return new List<CashTransactionDto>();
            }
        }

        public async Task<List<CashTransactionDto>> GetTodayAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/cash-transactions/today");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<CashTransactionDto>>() ?? new List<CashTransactionDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get today's cash transactions");
                return new List<CashTransactionDto>();
            }
        }

        public async Task<CashSummaryDto?> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var url = "api/cash-transactions/summary";
                var queryParams = new List<string>();
                
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                
                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);

                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<CashSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get cash summary");
                return null;
            }
        }

        public async Task<CashSummaryDto?> GetTodaySummaryAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/cash-transactions/summary/today");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<CashSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get today's cash summary");
                return null;
            }
        }

        public async Task<(bool Success, string Message, CashTransactionDto? Transaction)> CashInAsync(CreateCashTransactionDto dto)
        {
            try
            {
                var userName = POS.UI.Core.AppState.CurrentUserName ?? "System";
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"api/cash-transactions/cash-in?userId=1&userName={Uri.EscapeDataString(userName)}", content);

                if (response.IsSuccessStatusCode)
                {
                    var transaction = await response.Content.ReadFromJsonAsync<CashTransactionDto>();
                    return (true, "Cash added successfully", transaction);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, error, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add cash");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message, CashTransactionDto? Transaction)> CashOutAsync(CreateCashTransactionDto dto)
        {
            try
            {
                var userName = POS.UI.Core.AppState.CurrentUserName ?? "System";
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"api/cash-transactions/cash-out?userId=1&userName={Uri.EscapeDataString(userName)}", content);

                if (response.IsSuccessStatusCode)
                {
                    var transaction = await response.Content.ReadFromJsonAsync<CashTransactionDto>();
                    return (true, "Expense recorded successfully", transaction);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, error, null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to record expense");
                return (false, ex.Message, null);
            }
        }
    }
}
