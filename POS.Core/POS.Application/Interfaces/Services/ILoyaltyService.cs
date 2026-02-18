using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ILoyaltyService
{
    Task<LoyaltySettingsDto?> GetSettingsAsync();
    Task<LoyaltySettingsDto> SaveSettingsAsync(UpdateLoyaltySettingsDto dto);
    Task<int> CalculatePointsAsync(decimal amount);
    Task<RedeemPointsResponse> RedeemPointsAsync(Guid customerId, int points);
}

