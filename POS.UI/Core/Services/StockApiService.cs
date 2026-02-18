using Serilog;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Core.Services;

/// <summary>
/// API service for stock-related queries
/// </summary>
public class StockApiService
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StockApiService(HttpClient http)
    {
        _http = http;
        _logger = Log.ForContext<StockApiService>();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Get current stock quantity for a product
    /// </summary>
    public async Task<decimal> GetProductStockAsync(long productId)
    {
        try
        {
            // Get available stock from batches endpoint
            var batchStock = await _http.GetFromJsonAsync<decimal>($"api/batches/product/{productId}/stock/available", _jsonOptions);
            return batchStock;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to get stock for product {ProductId}, returning 0", productId);
            return 0;
        }
    }

    /// <summary>
    /// Get all products with low stock
    /// </summary>
    public async Task<List<LowStockProductDto>> GetLowStockProductsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<LowStockProductDto>>("api/stock/low-stock", _jsonOptions);
            return result ?? new List<LowStockProductDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get low stock products");
            return new List<LowStockProductDto>();
        }
    }
}

public class LowStockProductDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal AvailableStock { get; set; }
    public decimal ReorderLevel { get; set; }
}
