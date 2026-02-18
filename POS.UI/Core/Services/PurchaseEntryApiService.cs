using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class PurchaseEntryApiService : BaseApiService
{
    public PurchaseEntryApiService(HttpClient http) : base(http) { }

    public async Task<List<PurchaseEntryDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/purchase-entries?includeInactive=true" : "api/purchase-entries";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return new List<PurchaseEntryDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<PurchaseEntryDto>>(json, options);
            return result ?? new List<PurchaseEntryDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch purchase entries.", ex);
        }
    }

    public async Task<PurchaseEntryDto?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseEntry ID must be valid.", nameof(id));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<PurchaseEntryDto>($"api/purchase-entries/{id}", options);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch purchase entry {id}.", ex);
        }
    }

    public async Task<List<PurchaseEntryDto>> GetBySupplierAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<List<PurchaseEntryDto>>($"api/purchase-entries/supplier/{supplierId}", options);
            return result ?? new List<PurchaseEntryDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch entries for supplier {supplierId}.", ex);
        }
    }

    public async Task<List<PurchaseEntryDto>> GetUnprocessedAsync()
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<List<PurchaseEntryDto>>("api/purchase-entries/unprocessed", options);
            return result ?? new List<PurchaseEntryDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch unprocessed entries.", ex);
        }
    }

    public async Task<PurchaseEntryDto> CreateAsync(CreatePurchaseEntryDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(dto));
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("At least one item is required.", nameof(dto));
        
        _logger.Information("Creating new purchase entry for supplier: {SupplierId}", dto.SupplierId);
        var response = await _http.PostAsJsonAsync("api/purchase-entries", dto);
        await EnsureSuccessAsync(response, "CreatePurchaseEntry");
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await response.Content.ReadFromJsonAsync<PurchaseEntryDto>(options);
        _logger.Information("Purchase entry created successfully: {PurchaseEntryId}", result?.PurchaseEntryId);
        return result ?? throw new HttpRequestException("Failed to deserialize created purchase entry.");
    }

    public async Task<PurchaseEntryDto> UpdateAsync(Guid id, CreatePurchaseEntryDto dto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseEntry ID must be valid.", nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(dto));
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("At least one item is required.", nameof(dto));
        
        _logger.Information("Updating purchase entry: {PurchaseEntryId}", id);
        var response = await _http.PutAsJsonAsync($"api/purchase-entries/{id}", dto);
        await EnsureSuccessAsync(response, "UpdatePurchaseEntry");
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await response.Content.ReadFromJsonAsync<PurchaseEntryDto>(options);
        _logger.Information("Purchase entry updated successfully: {PurchaseEntryId}", id);
        return result ?? throw new HttpRequestException("Failed to deserialize updated purchase entry.");
    }

    /// <summary>
    /// CRITICAL: Process the purchase entry to update inventory
    /// </summary>
    public async Task<PurchaseEntryDto> ProcessEntryAsync(Guid id, bool updateProductPrices = true)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseEntry ID must be valid.", nameof(id));
        
        _logger.Information("Processing purchase entry: {PurchaseEntryId}", id);
        var response = await _http.PostAsync($"api/purchase-entries/{id}/process?updateProductPrices={updateProductPrices}", null);
        await EnsureSuccessAsync(response, "ProcessPurchaseEntry");
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = await response.Content.ReadFromJsonAsync<PurchaseEntryDto>(options);
        _logger.Information("Purchase entry processed successfully: {PurchaseEntryId}", id);
        return result ?? throw new HttpRequestException("Failed to deserialize processed purchase entry.");
    }

    public async Task DisableAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseEntry ID must be valid.", nameof(id));
        var response = await _http.DeleteAsync($"api/purchase-entries/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckInvoiceNoExistsAsync(string invoiceNo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo))
            return false;
        try
        {
            var url = $"api/purchase-entries/exists/invoice?invoiceNo={Uri.EscapeDataString(invoiceNo)}";
            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check invoice number availability.", ex);
        }
    }
}
