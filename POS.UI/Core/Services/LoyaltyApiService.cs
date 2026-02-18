using System.Net.Http;
using System.Net.Http.Json;
using POS.Shared.Models;

namespace POS.UI.Core.Services;

public class LoyaltyApiService : BaseApiService
{
    public LoyaltyApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<LoyaltySettingsDto?> GetSettingsAsync()
    {
        var response = await _http.GetAsync("api/loyalty/settings");
        await EnsureSuccessAsync(response, "Loyalty_GetSettings");
        return await response.Content.ReadFromJsonAsync<LoyaltySettingsDto>();
    }

    public async Task<LoyaltySettingsDto?> SaveSettingsAsync(UpdateLoyaltySettingsDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/loyalty/settings", dto);
        await EnsureSuccessAsync(response, "Loyalty_SaveSettings");
        return await response.Content.ReadFromJsonAsync<LoyaltySettingsDto>();
    }

    public async Task<int> CalculatePointsAsync(decimal amount)
    {
        var response = await _http.GetAsync($"api/loyalty/calculate?amount={amount}");
        await EnsureSuccessAsync(response, "Loyalty_CalculatePoints");
        var result = await response.Content.ReadFromJsonAsync<CalculatePointsResponse>();
        return result?.points ?? 0;
    }

    public async Task<RedeemPointsResponse?> RedeemPointsAsync(Guid customerId, int points)
    {
        var request = new RedeemPointsRequest
        {
            CustomerId = customerId,
            PointsToRedeem = points
        };

        var response = await _http.PostAsJsonAsync("api/loyalty/redeem", request);
        await EnsureSuccessAsync(response, "Loyalty_RedeemPoints");
        return await response.Content.ReadFromJsonAsync<RedeemPointsResponse>();
    }

    private class CalculatePointsResponse
    {
        public int points { get; set; }
    }
}
