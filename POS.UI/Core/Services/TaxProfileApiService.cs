using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class TaxProfileApiService : BaseApiService
    {
        public TaxProfileApiService(HttpClient httpClient) : base(httpClient) { }

        public async Task<List<TaxProfileDto>?> GetAllAsync(bool includeInactive = false)
        {
            return await _http.GetFromJsonAsync<List<TaxProfileDto>>($"api/taxprofiles?includeInactive={includeInactive}");
        }

        public async Task<TaxProfileDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<TaxProfileDto>($"api/taxprofiles/{id}");
        }

        public async Task<TaxProfileDto?> AddAsync(TaxProfileDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/taxprofiles", dto);
            await EnsureSuccessAsync(response, "CreateTaxProfile");
            return await response.Content.ReadFromJsonAsync<TaxProfileDto>();
        }

        public async Task<TaxProfileDto?> UpdateAsync(int id, TaxProfileDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/taxprofiles/{id}", dto);
            await EnsureSuccessAsync(response, "UpdateTaxProfile");
            return await response.Content.ReadFromJsonAsync<TaxProfileDto>();
        }

        public async Task<bool> DisableAsync(int id)
        {
            var response = await _http.PatchAsync($"api/taxprofiles/{id}/disable", null);
            await EnsureSuccessAsync(response, "DisableTaxProfile");
            return true;
        }
    }
}
