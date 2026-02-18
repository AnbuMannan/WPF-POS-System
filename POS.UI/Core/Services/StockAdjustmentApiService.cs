using POS.Shared.Models;
using Serilog;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class StockAdjustmentApiService
{
    private readonly HttpClient _http;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StockAdjustmentApiService(HttpClient http)
    {
        _http = http;
        _logger = Log.ForContext<StockAdjustmentApiService>();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<StockAdjustmentDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive 
                ? "api/stock-adjustments?includeInactive=true" 
                : "api/stock-adjustments";
            var result = await _http.GetFromJsonAsync<List<StockAdjustmentDto>>(url, _jsonOptions);
            return result ?? new List<StockAdjustmentDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch stock adjustments");
            throw;
        }
    }

    public async Task<StockAdjustmentDto?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<StockAdjustmentDto>($"api/stock-adjustments/{id}", _jsonOptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch stock adjustment {Id}", id);
            throw;
        }
    }

    public async Task<StockAdjustmentDto?> GetByReferenceNoAsync(string referenceNo)
    {
        try
        {
            return await _http.GetFromJsonAsync<StockAdjustmentDto>(
                $"api/stock-adjustments/reference/{Uri.EscapeDataString(referenceNo)}", _jsonOptions);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch stock adjustment by reference {ReferenceNo}", referenceNo);
            throw;
        }
    }

    public async Task<List<StockAdjustmentDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var url = $"api/stock-adjustments/by-date-range?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
            var result = await _http.GetFromJsonAsync<List<StockAdjustmentDto>>(url, _jsonOptions);
            return result ?? new List<StockAdjustmentDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch stock adjustments by date range");
            throw;
        }
    }

    public async Task<List<StockAdjustmentDto>> GetByReasonAsync(string reason)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<StockAdjustmentDto>>(
                $"api/stock-adjustments/by-reason/{Uri.EscapeDataString(reason)}", _jsonOptions);
            return result ?? new List<StockAdjustmentDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch stock adjustments by reason {Reason}", reason);
            throw;
        }
    }

    public async Task<List<StockAdjustmentDto>> GetByStatusAsync(string status)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<StockAdjustmentDto>>(
                $"api/stock-adjustments/by-status/{Uri.EscapeDataString(status)}", _jsonOptions);
            return result ?? new List<StockAdjustmentDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch stock adjustments by status {Status}", status);
            throw;
        }
    }

    public async Task<string[]> GetReasonsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<string[]>("api/stock-adjustments/reasons", _jsonOptions);
            return result ?? AdjustmentReasons.All;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch adjustment reasons");
            return AdjustmentReasons.All;
        }
    }

    /// <summary>
    /// Create and immediately approve a stock adjustment
    /// </summary>
    public async Task<StockAdjustmentDto> CreateAndApproveAsync(CreateStockAdjustmentDto dto, string approvedBy = "System")
    {
        try
        {
            _logger.Information("Creating stock adjustment with {ItemCount} items", dto.Items.Count);
            var response = await _http.PostAsJsonAsync(
                $"api/stock-adjustments?approvedBy={Uri.EscapeDataString(approvedBy)}", dto);
            await EnsureSuccessAsync(response, "CreateStockAdjustment");
            var result = await response.Content.ReadFromJsonAsync<StockAdjustmentDto>(_jsonOptions);
            _logger.Information("Stock adjustment created: {ReferenceNo}", result?.ReferenceNo);
            return result ?? throw new HttpRequestException("Failed to deserialize stock adjustment");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create stock adjustment");
            throw;
        }
    }

    /// <summary>
    /// Create a draft adjustment
    /// </summary>
    public async Task<StockAdjustmentDto> CreateDraftAsync(CreateStockAdjustmentDto dto)
    {
        try
        {
            _logger.Information("Creating draft stock adjustment");
            var response = await _http.PostAsJsonAsync("api/stock-adjustments/draft", dto);
            await EnsureSuccessAsync(response, "CreateDraftStockAdjustment");
            var result = await response.Content.ReadFromJsonAsync<StockAdjustmentDto>(_jsonOptions);
            return result ?? throw new HttpRequestException("Failed to deserialize stock adjustment");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create draft stock adjustment");
            throw;
        }
    }

    /// <summary>
    /// Approve and process a draft adjustment
    /// </summary>
    public async Task<StockAdjustmentDto> ApproveAsync(Guid id, string approvedBy = "System")
    {
        try
        {
            _logger.Information("Approving stock adjustment: {Id}", id);
            var response = await _http.PostAsync(
                $"api/stock-adjustments/{id}/approve?approvedBy={Uri.EscapeDataString(approvedBy)}", null);
            await EnsureSuccessAsync(response, "ApproveStockAdjustment");
            var result = await response.Content.ReadFromJsonAsync<StockAdjustmentDto>(_jsonOptions);
            return result ?? throw new HttpRequestException("Failed to deserialize stock adjustment");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to approve stock adjustment {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Cancel a draft adjustment
    /// </summary>
    public async Task CancelAsync(Guid id)
    {
        try
        {
            _logger.Information("Cancelling stock adjustment: {Id}", id);
            var response = await _http.PostAsync($"api/stock-adjustments/{id}/cancel", null);
            await EnsureSuccessAsync(response, "CancelStockAdjustment");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to cancel stock adjustment {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete (disable) an adjustment
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            _logger.Information("Deleting stock adjustment: {Id}", id);
            var response = await _http.DeleteAsync($"api/stock-adjustments/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete stock adjustment {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Validate stock for a potential adjustment
    /// </summary>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateStockAsync(CreateStockAdjustmentDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/stock-adjustments/validate", dto);
            var result = await response.Content.ReadFromJsonAsync<ValidationResult>(_jsonOptions);
            return (result?.IsValid ?? false, result?.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to validate stock adjustment");
            return (false, ex.Message);
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operation)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.Error("API error during {Operation}: {StatusCode} - {Content}", 
                operation, response.StatusCode, content);
            throw new HttpRequestException($"{operation} failed: {content}");
        }
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
