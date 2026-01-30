using POS.UI.Core.Exceptions;
using POS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace POS.UI.Core.Services;

public class CategoryApiService : BaseApiService
{
    public CategoryApiService(HttpClient http) : base(http) { }

    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var url = includeInactive ? "api/categories/all?includeInactive=true" : "api/categories/all";
            var fallback = includeInactive ? "api/categories?includeInactive=true" : "api/categories";
            var json = await TryGetJsonAsync(url, fallback);
            if (string.IsNullOrWhiteSpace(json))
                return new List<CategoryDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<List<CategoryDto>>(json, options);
            return result ?? new List<CategoryDto>();
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to fetch categories.", ex);
        }
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Category ID must be greater than 0.", nameof(id));
        try
        {
            var result = await _http.GetFromJsonAsync<CategoryDto>($"api/categories/{id}");
            return result ?? throw new HttpRequestException($"Category with ID {id} not found.");
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException($"Failed to fetch category {id}.", ex);
        }
    }

    public async Task AddAsync(CategoryDto category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name cannot be empty.", nameof(category));
        _logger.Information("Creating new category: {CategoryName} (Parent: {ParentId})", category.Name, category.ParentCategoryId ?? 0);
        var response = await _http.PostAsJsonAsync("api/categories", category);
        await EnsureSuccessAsync(response, "CreateCategory");
        _logger.Information("Category created successfully: {CategoryId} - {CategoryName}", category.CategoryId, category.Name);
    }

    public async Task UpdateAsync(CategoryDto category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));
        if (category.CategoryId <= 0)
            throw new ArgumentException("Category ID must be greater than 0.", nameof(category));
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name cannot be empty.", nameof(category));
        _logger.Information("Updating category: {CategoryId} - {CategoryName}", category.CategoryId, category.Name);
        var response = await _http.PutAsJsonAsync("api/categories", category);
        await EnsureSuccessAsync(response, "UpdateCategory");
        _logger.Information("Category updated successfully: {CategoryId}", category.CategoryId);
    }

    public async Task DisableAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Category ID cannot be empty.", nameof(id));
        var response = await _http.DeleteAsync($"api/categories/{id}");
        await EnsureSuccessAsync(response);
    }

    public async Task<bool> CheckNameExistsAsync(string name, int? parentId = null, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        try
        {
            var url = $"api/categories/exists?name={Uri.EscapeDataString(name)}";
            if (parentId.HasValue && parentId.Value > 0)
                url += $"&parentCategoryId={parentId}";
            if (excludeId.HasValue && excludeId.Value > 0)
                url += $"&excludeId={excludeId}";
            var result = await _http.GetFromJsonAsync<bool>(url);
            return result;
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            throw new HttpRequestException("Failed to check category name availability.", ex);
        }
    }
}
