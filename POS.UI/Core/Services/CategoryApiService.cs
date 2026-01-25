using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using POS.UI.Core.Models;
using POS.UI.Modules.Admin.Categories;

namespace POS.UI.Core.Services
{
    public class CategoryApiService
    {
        private readonly HttpClient _http;

        public CategoryApiService(HttpClient http)
        {
            _http = http;
        }

        // ================= LIST =================

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories");
        }

        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<CategoryDto>($"api/categories/{id}");
        }

        // ================= CREATE =================

        public async Task AddAsync(CategoryDto category)
        {
            var response = await _http.PostAsJsonAsync("api/categories", category);
            response.EnsureSuccessStatusCode();
        }

        // ================= UPDATE =================

        public async Task UpdateAsync(CategoryDto category)
        {
            var response = await _http.PutAsJsonAsync($"api/categories/{category.CategoryId}", category);
            response.EnsureSuccessStatusCode();
        }

        // ================= DISABLE (SOFT DELETE) =================

        public async Task DisableAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/categories/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ================= DUPLICATE CHECK =================

        public async Task<bool> CheckNameExistsAsync(string name, Guid? parentId, Guid? excludeId)
        {
            var url = $"api/categories/exists?name={Uri.EscapeDataString(name)}";

            if (parentId.HasValue)
                url += $"&parentId={parentId}";

            if (excludeId.HasValue)
                url += $"&excludeId={excludeId}";

            return await _http.GetFromJsonAsync<bool>(url);
        }
    }
}
