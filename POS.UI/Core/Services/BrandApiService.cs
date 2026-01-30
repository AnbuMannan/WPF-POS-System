using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class BrandApiService : BaseApiService
{
    public BrandApiService(HttpClient http) : base(http) { }

    public async Task<List<BrandDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/brands/all?includeInactive=true" : "api/brands/all";
            var fallback = includeInactive ? "api/brands?includeInactive=true" : "api/brands";
            var json = await TryGetJsonAsync(url, fallback);
            if (string.IsNullOrWhiteSpace(json))
                return new List<BrandDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<BrandDto>>(json, options);
            return result ?? new List<BrandDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch brands.", ex);
        }
    }

    public async Task<BrandDto> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Brand ID must be greater than 0.", nameof(id));
        try
        {
            var result = await _http.GetFromJsonAsync<BrandDto>($"api/brands/{id}");
            return result ?? throw new HttpRequestException($"Brand with ID {id} not found.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch brand {id}.", ex);
        }
    }

    public async Task AddAsync(BrandDto brand)
    {
        if (brand == null)
            throw new ArgumentNullException(nameof(brand));
        if (string.IsNullOrWhiteSpace(brand.Name))
            throw new ArgumentException("Brand name cannot be empty.", nameof(brand));
        _logger.Information("Creating new brand: {BrandName}", brand.Name);
        var response = await _http.PostAsJsonAsync("api/brands", brand);
        await EnsureSuccessAsync(response, "CreateBrand");
        _logger.Information("Brand created successfully: {BrandId} - {BrandName}", brand.BrandId, brand.Name);
    }

    public async Task UpdateAsync(BrandDto brand)
    {
        if (brand == null)
            throw new ArgumentNullException(nameof(brand));
        if (brand.BrandId <= 0)
            throw new ArgumentException("Brand ID must be greater than 0.", nameof(brand));
        if (string.IsNullOrWhiteSpace(brand.Name))
            throw new ArgumentException("Brand name cannot be empty.", nameof(brand));
        _logger.Information("Updating brand: {BrandId} - {BrandName}", brand.BrandId, brand.Name);
        var response = await _http.PutAsJsonAsync("api/brands", brand);
        await EnsureSuccessAsync(response, "UpdateBrand");
        _logger.Information("Brand updated successfully: {BrandId}", brand.BrandId);
    }

    public async Task DisableAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Brand ID must be greater than 0.", nameof(id));
        var response = await _http.DeleteAsync($"api/brands/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckNameExistsAsync(string name, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        try
        {
            var url = $"api/brands/exists/name?name={Uri.EscapeDataString(name)}";
            if (excludeId.HasValue && excludeId.Value > 0)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check brand name availability.", ex);
        }
    }
}
