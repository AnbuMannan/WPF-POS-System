using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class EODReportApiService : BaseApiService
    {
        public EODReportApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<EODReportDto?> GetEODReportAsync(DateTime date)
        {
            return await _http.GetFromJsonAsync<EODReportDto>($"api/eod-reports?date={date:yyyy-MM-dd}");
        }

        public async Task CloseDayAsync(DateTime date)
        {
            var response = await _http.PostAsync($"api/eod-reports/close-day?date={date:yyyy-MM-dd}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
