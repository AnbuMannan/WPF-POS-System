using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class UomApiService : BaseApiService
    {
        public UomApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<UomDto>?> GetAllAsync(bool includeInactive = false)
        {
            return await _http.GetFromJsonAsync<List<UomDto>>($"api/uoms?includeInactive={includeInactive}");
        }

        public async Task<UomDto?> GetByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<UomDto>($"api/uoms/{id}");
        }

        public async Task<UomDto?> CreateAsync(UomDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/uoms", dto);
            await EnsureSuccessAsync(response, "CreateUom");
            return await response.Content.ReadFromJsonAsync<UomDto>();
        }

        public async Task<UomDto?> UpdateAsync(Guid id, UomDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/uoms/{id}", dto);
            await EnsureSuccessAsync(response, "UpdateUom");
            return await response.Content.ReadFromJsonAsync<UomDto>();
        }

        public async Task<bool> DisableAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/uoms/{id}");
            await EnsureSuccessAsync(response, "DisableUom");
            return true;
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        {
            var url = $"api/uoms/exists/code?code={Uri.EscapeDataString(code ?? "")}";
            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
                url += $"&excludeId={excludeId.Value}";
            return await _http.GetFromJsonAsync<bool>(url);
        }
    }
}
