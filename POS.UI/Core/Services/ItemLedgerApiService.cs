using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using POS.Shared.Models;
using Serilog;

namespace POS.UI.Core.Services;

/// <summary>
/// API service for item ledger operations
/// </summary>
public class ItemLedgerApiService
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ItemLedgerApiService(HttpClient http)
    {
        _http = http;
        _logger = Log.ForContext<ItemLedgerApiService>();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Get complete item ledger with summary for a product
    /// </summary>
    public async Task<ItemLedgerResponseDto?> GetLedgerAsync(long productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var url = $"api/inventory/ledger/{productId}";
            var queryParams = new List<string>();
            
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            _logger.Information("Fetching item ledger for product {ProductId} from {FromDate} to {ToDate}", 
                productId, fromDate, toDate);

            var result = await _http.GetFromJsonAsync<ItemLedgerResponseDto>(url, _jsonOptions);
            
            _logger.Information("Retrieved {EntryCount} ledger entries for product {ProductId}", 
                result?.Entries?.Count ?? 0, productId);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.Error(ex, "HTTP error fetching item ledger for product {ProductId}", productId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching item ledger for product {ProductId}", productId);
            throw;
        }
    }

    /// <summary>
    /// Get ledger entries only (without summary)
    /// </summary>
    public async Task<List<ItemLedgerDto>> GetEntriesAsync(long productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var url = $"api/inventory/ledger/{productId}/entries";
            var queryParams = new List<string>();
            
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            var result = await _http.GetFromJsonAsync<List<ItemLedgerDto>>(url, _jsonOptions);
            return result ?? new List<ItemLedgerDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching ledger entries for product {ProductId}", productId);
            return new List<ItemLedgerDto>();
        }
    }
}
