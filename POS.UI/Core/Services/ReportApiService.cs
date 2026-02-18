using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class ReportApiService : BaseApiService
    {
        public ReportApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<SalesSummaryReportRow>> GetSalesReportAsync(DateTime from, DateTime to, Guid? customerId, string? status)
        {
            var url = $"api/reports/sales?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            if (customerId.HasValue && customerId.Value != Guid.Empty)
                url += $"&customerId={customerId.Value}";
            if (!string.IsNullOrWhiteSpace(status))
                url += $"&status={Uri.EscapeDataString(status)}";
            var result = await _http.GetFromJsonAsync<List<SalesSummaryReportRow>>(url);
            return result ?? new List<SalesSummaryReportRow>();
        }

        public async Task<List<ItemWiseSalesRow>> GetItemWiseSalesAsync(DateTime from, DateTime to, int? categoryId)
        {
            var url = $"api/reports/item-wise-sales?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            if (categoryId.HasValue && categoryId.Value > 0)
                url += $"&categoryId={categoryId.Value}";
            var result = await _http.GetFromJsonAsync<List<ItemWiseSalesRow>>(url);
            return result ?? new List<ItemWiseSalesRow>();
        }

        public async Task<ProfitLossReportDto> GetProfitLossAsync(DateTime from, DateTime to)
        {
            var url = $"api/reports/profit-loss?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            var result = await _http.GetFromJsonAsync<ProfitLossReportDto>(url);
            return result ?? new ProfitLossReportDto { From = from, To = to };
        }

        public async Task<List<LowStockItemRow>> GetLowStockAsync(decimal threshold)
        {
            var url = $"api/reports/low-stock?threshold={threshold.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            var result = await _http.GetFromJsonAsync<List<LowStockItemRow>>(url);
            return result ?? new List<LowStockItemRow>();
        }
    }
}

