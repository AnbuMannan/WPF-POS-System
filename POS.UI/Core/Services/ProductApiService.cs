using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services;

public class ProductApiService : BaseApiService
{
    public ProductApiService(HttpClient http) : base(http) { }

    public async Task<List<ProductDto>> GetAllAsync(bool showInactive = false)
    {
        try
        {
            _logger.Debug("Fetching all products: showInactive={ShowInactive}", showInactive);
            var result = await _http.GetFromJsonAsync<List<ProductDto>>(
                $"api/products/all{(showInactive ? "?showInactive=true" : string.Empty)}");
            var list = result ?? new List<ProductDto>();
            _logger.Information("Successfully fetched {ProductCount} products", list.Count);
            return list;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            _logger.Error(ex, "Failed to fetch products");
            throw new HttpRequestException("Failed to fetch products.", ex);
        }
    }

    public async Task<ProductDto> GetByIdAsync(long id)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ProductDto>($"api/products/{id}");
            return result ?? throw new HttpRequestException($"Product with ID {id} not found.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch product {id}.", ex);
        }
    }

    public async Task<ProductDto> GetByBarcodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Barcode cannot be empty.", nameof(code));
        try
        {
            var result = await _http.GetFromJsonAsync<ProductDto>(
                $"api/products/barcode/{Uri.EscapeDataString(code)}");
            return result ?? throw new HttpRequestException($"Product with barcode {code} not found.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch product by barcode.", ex);
        }
    }

    public async Task<List<ProductDto>> SearchAsync(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return new List<ProductDto>();
        try
        {
            var result = await _http.GetFromJsonAsync<List<ProductDto>>(
                $"api/products/search?q={Uri.EscapeDataString(q)}");
            return result ?? new List<ProductDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Search failed.", ex);
        }
    }

    public async Task CreateAsync(ProductDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        _logger.Information("Creating new product: {ProductName} (SKU: {SKU})", dto.Name, dto.SKU);
        var response = await _http.PostAsJsonAsync("api/products", dto);
        await EnsureSuccessAsync(response, "CreateProduct");
        _logger.Information("Product created successfully: {ProductName}", dto.Name);
    }

    public async Task UpdateAsync(ProductDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (dto.ProductId <= 0)
            throw new ArgumentException("Product ID must be greater than 0.", nameof(dto));
        _logger.Information("Updating product: {ProductId} - {ProductName} (SKU: {SKU})", dto.ProductId, dto.Name, dto.SKU);
        var response = await _http.PutAsJsonAsync("api/products", dto);
        await EnsureSuccessAsync(response, "UpdateProduct");
        _logger.Information("Product updated successfully: {ProductId}", dto.ProductId);
    }

    public async Task DisableAsync(long id)
    {
        if (id <= 0)
            throw new ArgumentException("Product ID must be greater than 0.", nameof(id));
        _logger.Information("Disabling product: {ProductId}", id);
        var response = await _http.DeleteAsync($"api/products/{id}");
        await EnsureSuccessAsync(response, "DisableProduct");
        _logger.Information("Product disabled successfully: {ProductId}", id);
    }

    public async Task<bool> CheckSkuExistsAsync(string sku, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;
        try
        {
            var url = $"api/products/exists/sku?sku={Uri.EscapeDataString(sku)}";
            if (excludeId.HasValue && excludeId.Value > 0)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check SKU availability.", ex);
        }
    }

    public async Task<bool> CheckBarcodeExistsAsync(string barcode, long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;
        try
        {
            var url = $"api/products/exists/barcode?barcode={Uri.EscapeDataString(barcode)}";
            if (excludeId.HasValue && excludeId.Value > 0)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check barcode availability.", ex);
        }
    }
}
