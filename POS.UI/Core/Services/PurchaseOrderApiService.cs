using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using POS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class PurchaseOrderApiService : BaseApiService
{
    public PurchaseOrderApiService(HttpClient http) : base(http) { }

    public async Task<List<PurchaseOrderDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/purchase-orders?includeInactive=true" : "api/purchase-orders";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return new List<PurchaseOrderDto>();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var result = JsonSerializer.Deserialize<List<PurchaseOrderDto>>(json, options);
            return result ?? new List<PurchaseOrderDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch purchase orders.", ex);
        }
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseOrder ID must be valid.", nameof(id));
        try
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var result = await _http.GetFromJsonAsync<PurchaseOrderDto>($"api/purchase-orders/{id}", options);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch purchase order {id}.", ex);
        }
    }

    public async Task<List<PurchaseOrderDto>> GetPendingBySupplierAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var result = await _http.GetFromJsonAsync<List<PurchaseOrderDto>>($"api/purchase-orders/supplier/{supplierId}/pending", options);
            return result ?? new List<PurchaseOrderDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch pending orders for supplier {supplierId}.", ex);
        }
    }

    public async Task<List<PurchaseOrderDto>> GetByStatusAsync(PurchaseOrderStatus status)
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            var result = await _http.GetFromJsonAsync<List<PurchaseOrderDto>>($"api/purchase-orders/status/{status}", options);
            return result ?? new List<PurchaseOrderDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch orders with status {status}.", ex);
        }
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(dto));
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("At least one item is required.", nameof(dto));
        
        _logger.Information("Creating new purchase order for supplier: {SupplierId}", dto.SupplierId);
        var response = await _http.PostAsJsonAsync("api/purchase-orders", dto);
        await EnsureSuccessAsync(response, "CreatePurchaseOrder");
        
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        var result = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(options);
        _logger.Information("Purchase order created successfully: {PurchaseOrderId}", result?.PurchaseOrderId);
        return result ?? throw new HttpRequestException("Failed to deserialize created purchase order.");
    }

    public async Task<PurchaseOrderDto> UpdateAsync(Guid id, CreatePurchaseOrderDto dto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseOrder ID must be valid.", nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.SupplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(dto));
        if (dto.Items == null || dto.Items.Count == 0)
            throw new ArgumentException("At least one item is required.", nameof(dto));
        
        _logger.Information("Updating purchase order: {PurchaseOrderId}", id);
        var response = await _http.PutAsJsonAsync($"api/purchase-orders/{id}", dto);
        await EnsureSuccessAsync(response, "UpdatePurchaseOrder");
        
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
        var result = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>(options);
        _logger.Information("Purchase order updated successfully: {PurchaseOrderId}", id);
        return result ?? throw new HttpRequestException("Failed to deserialize updated purchase order.");
    }

    public async Task UpdateStatusAsync(Guid id, PurchaseOrderStatus status)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseOrder ID must be valid.", nameof(id));
        
        _logger.Information("Updating purchase order status: {PurchaseOrderId} to {Status}", id, status);
        var response = await _http.PatchAsync($"api/purchase-orders/{id}/status", 
            JsonContent.Create(status));
        await EnsureSuccessAsync(response, "UpdatePurchaseOrderStatus");
        _logger.Information("Purchase order status updated successfully: {PurchaseOrderId}", id);
    }

    public async Task DisableAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseOrder ID must be valid.", nameof(id));
        var response = await _http.DeleteAsync($"api/purchase-orders/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckReferenceNoExistsAsync(string referenceNo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(referenceNo))
            return false;
        try
        {
            var url = $"api/purchase-orders/exists/reference?referenceNo={Uri.EscapeDataString(referenceNo)}";
            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check reference number availability.", ex);
        }
    }
}
