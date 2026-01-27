using POS.UI.Core.Exceptions;
using POS.UI.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services
{
    /// <summary>
    /// API service for Category CRUD operations and hierarchy management.
    /// </summary>
    public class CategoryApiService : BaseApiService
    {
        public CategoryApiService(HttpClient http) : base(http)
        {
        }

        /// <summary>
        /// Retrieves all categories with their hierarchy information.
        /// </summary>
        public async Task<List<CategoryDto>> GetAllAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories");
                return result ?? new List<CategoryDto>();
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new HttpRequestException("Failed to fetch categories.", ex);
            }
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty.", nameof(id));

            try
            {
                var result = await _http.GetFromJsonAsync<CategoryDto>($"api/categories/{id}");
                return result ?? throw new HttpRequestException($"Category with ID {id} not found.");
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new HttpRequestException($"Failed to fetch category {id}.", ex);
            }
        }

        /// <summary>
        /// Adds a new category (root or sub-category).
        /// </summary>
        public async Task AddAsync(CategoryDto category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be empty.", nameof(category));

            _logger.Information("Creating new category: {CategoryName} (Parent: {ParentId})", 
                category.Name, category.ParentCategoryId ?? Guid.Empty);
            var response = await _http.PostAsJsonAsync("api/categories", category);
            await EnsureSuccessAsync(response, "CreateCategory");
            _logger.Information("Category created successfully: {CategoryId} - {CategoryName}", 
                category.CategoryId, category.Name);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        public async Task UpdateAsync(CategoryDto category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (category.CategoryId == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty.", nameof(category));

            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be empty.", nameof(category));

            _logger.Information("Updating category: {CategoryId} - {CategoryName}", 
                category.CategoryId, category.Name);
            var response = await _http.PutAsJsonAsync($"api/categories/{category.CategoryId}", category);
            await EnsureSuccessAsync(response, "UpdateCategory");
            _logger.Information("Category updated successfully: {CategoryId}", category.CategoryId);
        }

        /// <summary>
        /// Disables (soft deletes) a category.
        /// </summary>
        public async Task DisableAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Category ID cannot be empty.", nameof(id));

            var response = await _http.DeleteAsync($"api/categories/{id}");
            await EnsureSuccessAsync(response);
        }

        /// <summary>
        /// Checks if a category name already exists at the specified parent level.
        /// </summary>
        public async Task<bool> CheckNameExistsAsync(string name, Guid? parentId = null, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            try
            {
                var url = $"api/categories/exists?name={Uri.EscapeDataString(name)}";

                if (parentId.HasValue && parentId != Guid.Empty)
                    url += $"&parentId={parentId}";

                if (excludeId.HasValue && excludeId != Guid.Empty)
                    url += $"&excludeId={excludeId}";

                var result = await _http.GetFromJsonAsync<bool>(url);
                return result;
            }
            catch (Exception ex) when (!(ex is HttpRequestException))
            {
                throw new HttpRequestException("Failed to check category name availability.", ex);
            }
        }
    }
}
