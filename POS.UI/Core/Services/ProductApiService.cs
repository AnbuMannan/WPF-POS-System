using POS.UI.Core.Exceptions;
using POS.UI.Modules.Admin.Products.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static POS.UI.Modules.Admin.Products.ProductFormView;

namespace POS.UI.Core.Services
{
    public class ProductApiService
    {
        private readonly HttpClient _http;

        public ProductApiService(HttpClient http)
        {
            _http = http;
        }

        public Task<List<CategoryDto>> GetAllAsync(bool showInactive = false)
            => _http.GetFromJsonAsync<List<CategoryDto>>($"api/products/all?showInactive={showInactive}")!;


        public Task<CategoryDto> GetByIdAsync(Guid id)
            => _http.GetFromJsonAsync<CategoryDto>($"api/products/{id}")!;

        public Task<CategoryDto> GetByBarcodeAsync(string code)
            => _http.GetFromJsonAsync<CategoryDto>($"api/products/barcode/{code}")!;

        public Task<List<CategoryDto>> SearchAsync(string q)
            => _http.GetFromJsonAsync<List<CategoryDto>>($"api/products/search?q={q}")!;

        public async Task CreateAsync(CategoryDto dto)
        {
            var response = await _http.PostAsJsonAsync($"api/products", dto);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiValidationError>();
                    throw new ApiValidationException(error);
                }

                response.EnsureSuccessStatusCode();
            }
        }



        public async Task UpdateAsync(CategoryDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/products", dto);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiValidationError>();
                    throw new ApiValidationException(error);
                }

                response.EnsureSuccessStatusCode();
            }
        }
        public async Task DisableAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/products/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> CheckSkuExistsAsync(string sku, Guid? excludeId)
        {
            var url = $"api/products/exists/sku?sku={Uri.EscapeDataString(sku)}";

            if (excludeId != null)
                url += $"&excludeId={excludeId}";

            return await _http.GetFromJsonAsync<bool>(url);
        }

        public async Task<bool> CheckBarcodeExistsAsync(string barcode, Guid? excludeId)
        {
            var url = $"api/products/exists/barcode?barcode={Uri.EscapeDataString(barcode)}";

            if (excludeId != null)
                url += $"&excludeId={excludeId}";

            return await _http.GetFromJsonAsync<bool>(url);
        }

    }

}
