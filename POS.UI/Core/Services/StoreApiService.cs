using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using POS.Shared.Models;

namespace POS.UI.Core.Services
{
    public class StoreApiService : BaseApiService
    {
        public StoreApiService(HttpClient http) : base(http) { }

        public async Task<List<StoreDto>> GetStoresAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<StoreDto>>("api/stores");
                return response ?? new List<StoreDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch stores");
                throw new HttpRequestException("Failed to fetch available stores.", ex);
            }
        }

        public async Task<StoreDto> CreateStoreAsync(CreateStoreDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/stores", dto);
                await EnsureSuccessAsync(response, "CreateStore");
                return await response.Content.ReadFromJsonAsync<StoreDto>() ?? throw new Exception("Invalid store response");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create store");
                throw new HttpRequestException("Failed to create store.", ex);
            }
        }

        public async Task SyncStoreAsync(StoreDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/stores/sync", dto);
                await EnsureSuccessAsync(response, "SyncStore");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to sync store to core");
                throw new HttpRequestException("Failed to sync store to core database.", ex);
            }
        }
    }
}