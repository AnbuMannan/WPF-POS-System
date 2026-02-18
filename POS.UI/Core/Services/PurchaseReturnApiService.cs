using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class PurchaseReturnApiService : BaseApiService
{
    public PurchaseReturnApiService(HttpClient http) : base(http) { }

    public async Task<List<PurchaseReturnDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/purchase-returns?includeInactive=true" : "api/purchase-returns";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return new List<PurchaseReturnDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<PurchaseReturnDto>>(json, options);
            return result ?? new List<PurchaseReturnDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch purchase returns.", ex);
        }
    }

    public async Task<PurchaseReturnDto?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseReturn ID must be valid.", nameof(id));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<PurchaseReturnDto>($"api/purchase-returns/{id}", options);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch purchase return {id}.", ex);
        }
    }

    public async Task<List<PurchaseReturnDto>> GetBySupplierAsync(Guid supplierId)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID must be valid.", nameof(supplierId));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<List<PurchaseReturnDto>>($"api/purchase-returns/supplier/{supplierId}", options);
            return result ?? new List<PurchaseReturnDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch returns for supplier {supplierId}.", ex);
        }
    }

    public async Task<List<PurchaseReturnDto>> GetByPurchaseEntryAsync(Guid purchaseEntryId)
    {
        if (purchaseEntryId == Guid.Empty)
            throw new ArgumentException("PurchaseEntry ID must be valid.", nameof(purchaseEntryId));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<List<PurchaseReturnDto>>($"api/purchase-returns/purchase-entry/{purchaseEntryId}", options);
            return result ?? new List<PurchaseReturnDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch returns for purchase entry {purchaseEntryId}.", ex);
        }
    }

    public async Task<List<PurchaseReturnDto>> GetUnprocessedAsync()
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await _http.GetFromJsonAsync<List<PurchaseReturnDto>>("api/purchase-returns/unprocessed", options);
            return result ?? new List<PurchaseReturnDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch unprocessed purchase returns.", ex);
        }
    }

    public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await _http.PostAsJsonAsync("api/purchase-returns", dto);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to create purchase return: {jsonResponse}");
            }

            var result = JsonSerializer.Deserialize<PurchaseReturnDto>(jsonResponse, options);
            return result ?? throw new InvalidOperationException("Failed to create purchase return.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to create purchase return.", ex);
        }
    }

    public async Task<PurchaseReturnDto> UpdateAsync(Guid id, CreatePurchaseReturnDto dto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseReturn ID must be valid.", nameof(id));
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await _http.PutAsJsonAsync($"api/purchase-returns/{id}", dto);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to update purchase return: {jsonResponse}");
            }

            var result = JsonSerializer.Deserialize<PurchaseReturnDto>(jsonResponse, options);
            return result ?? throw new InvalidOperationException("Failed to update purchase return.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to update purchase return {id}.", ex);
        }
    }

    public async Task<PurchaseReturnDto> ProcessReturnAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseReturn ID must be valid.", nameof(id));
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await _http.PostAsync($"api/purchase-returns/{id}/process", null);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to process purchase return: {jsonResponse}");
            }

            var result = JsonSerializer.Deserialize<PurchaseReturnDto>(jsonResponse, options);
            return result ?? throw new InvalidOperationException("Failed to process purchase return.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to process purchase return {id}.", ex);
        }
    }

    public async Task<bool> DisableAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("PurchaseReturn ID must be valid.", nameof(id));
        try
        {
            var response = await _http.DeleteAsync($"api/purchase-returns/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to disable purchase return: {errorContent}");
            }
            return true;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to disable purchase return {id}.", ex);
        }
    }

    public async Task<bool> CheckReturnNoExistsAsync(string returnNo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(returnNo))
            return false;
        try
        {
            var url = excludeId.HasValue
                ? $"api/purchase-returns/check-return-no/{returnNo}?excludeId={excludeId.Value}"
                : $"api/purchase-returns/check-return-no/{returnNo}";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await _http.GetFromJsonAsync<Dictionary<string, bool>>(url, options);
            return response != null && response.ContainsKey("exists") && response["exists"];
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to check return number {returnNo}.", ex);
        }
    }
}
