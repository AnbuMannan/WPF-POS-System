using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface ILoyaltySettingsRepository
{
    Task<LoyaltySetting?> GetAsync();
    Task<LoyaltySetting> CreateOrUpdateAsync(LoyaltySetting entity);
}

