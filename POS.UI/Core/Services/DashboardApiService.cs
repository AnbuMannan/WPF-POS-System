using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services;

public class DashboardApiService : BaseApiService
{
    public DashboardApiService(HttpClient http) : base(http) { }

    public async Task<DashboardSummaryDto?> GetSummaryAsync(DateTime date)
    {
        var url = $"api/dashboard/summary?date={date:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<DashboardSummaryDto>(url);
    }
}

