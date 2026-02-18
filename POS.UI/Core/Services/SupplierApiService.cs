using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class SupplierApiService : BaseApiService
{
    public SupplierApiService(HttpClient http) : base(http) { }

    public async Task<List<SupplierDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/suppliers/all?includeInactive=true" : "api/suppliers/all";
            var fallback = includeInactive ? "api/suppliers?includeInactive=true" : "api/suppliers";
            var json = await TryGetJsonAsync(url, fallback);
            if (string.IsNullOrWhiteSpace(json))
                return new List<SupplierDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<SupplierDto>>(json, options);
            return result ?? new List<SupplierDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch suppliers.", ex);
        }
    }

    public async Task<SupplierDto> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(id));
        try
        {
            var result = await _http.GetFromJsonAsync<SupplierDto>($"api/suppliers/{id}");
            return result ?? throw new HttpRequestException($"Supplier with ID {id} not found.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch supplier {id}.", ex);
        }
    }

    public async Task AddAsync(SupplierDto supplier)
    {
        if (supplier == null)
            throw new ArgumentNullException(nameof(supplier));
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new ArgumentException("Supplier name cannot be empty.", nameof(supplier));
        if (string.IsNullOrWhiteSpace(supplier.Code))
            throw new ArgumentException("Supplier code cannot be empty.", nameof(supplier));
        _logger.Information("Creating new supplier: {SupplierName}", supplier.Name);
        var response = await _http.PostAsJsonAsync("api/suppliers", supplier);
        await EnsureSuccessAsync(response, "CreateSupplier");
        _logger.Information("Supplier created successfully: {SupplierId} - {SupplierName}", supplier.Id, supplier.Name);
    }

    public async Task UpdateAsync(SupplierDto supplier)
    {
        if (supplier == null)
            throw new ArgumentNullException(nameof(supplier));
        if (supplier.Id == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplier));
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new ArgumentException("Supplier name cannot be empty.", nameof(supplier));
        if (string.IsNullOrWhiteSpace(supplier.Code))
            throw new ArgumentException("Supplier code cannot be empty.", nameof(supplier));
        _logger.Information("Updating supplier: {SupplierId} - {SupplierName}", supplier.Id, supplier.Name);
        var response = await _http.PutAsJsonAsync("api/suppliers", supplier);
        await EnsureSuccessAsync(response, "UpdateSupplier");
        _logger.Information("Supplier updated successfully: {SupplierId}", supplier.Id);
    }

    public async Task DisableAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(id));
        var response = await _http.DeleteAsync($"api/suppliers/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckCodeExistsAsync(string code, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;
        try
        {
            var url = $"api/suppliers/exists/code?code={Uri.EscapeDataString(code)}";
            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check supplier code availability.", ex);
        }
    }

    /// <summary>
    /// Get supplier ledger report with opening/closing balances for a date range
    /// </summary>
    public async Task<SupplierLedgerReportDto?> GetLedgerAsync(Guid supplierId, DateTime fromDate, DateTime toDate)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var url = $"api/suppliers/{supplierId}/ledger?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
            var result = await _http.GetFromJsonAsync<SupplierLedgerReportDto>(url);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch ledger for supplier {supplierId}.", ex);
        }
    }

    /// <summary>
    /// Get supplier balance as of a specific date
    /// </summary>
    public async Task<decimal> GetBalanceAsOfDateAsync(Guid supplierId, DateTime? asOfDate = null)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var url = $"api/suppliers/{supplierId}/balance";
            if (asOfDate.HasValue)
                url += $"?asOfDate={asOfDate.Value:yyyy-MM-dd}";
            var result = await _http.GetFromJsonAsync<decimal>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch balance for supplier {supplierId}.", ex);
        }
    }
}
