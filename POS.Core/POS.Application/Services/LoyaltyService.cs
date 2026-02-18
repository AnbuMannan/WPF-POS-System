using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.Application.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly ILoyaltySettingsRepository _settingsRepository;
    private readonly ICustomerRepository _customerRepository;

    public LoyaltyService(ILoyaltySettingsRepository settingsRepository, ICustomerRepository customerRepository)
    {
        _settingsRepository = settingsRepository;
        _customerRepository = customerRepository;
    }

    public async Task<LoyaltySettingsDto?> GetSettingsAsync()
    {
        var entity = await _settingsRepository.GetAsync();
        return entity == null
            ? null
            : new LoyaltySettingsDto
            {
                Id = entity.Id,
                PointsPerUnitCurrency = entity.PointsPerUnitCurrency,
                RedemptionValuePerPoint = entity.RedemptionValuePerPoint,
                MinimumRedeemPoints = entity.MinimumRedeemPoints
            };
    }

    public async Task<LoyaltySettingsDto> SaveSettingsAsync(UpdateLoyaltySettingsDto dto)
    {
        if (dto.PointsPerUnitCurrency < 0)
            throw new ArgumentException("Points per unit currency cannot be negative.");

        if (dto.RedemptionValuePerPoint < 0)
            throw new ArgumentException("Redemption value per point cannot be negative.");

        if (dto.MinimumRedeemPoints < 0)
            throw new ArgumentException("Minimum redeem points cannot be negative.");

        var entity = new Domain.Entities.LoyaltySetting
        {
            PointsPerUnitCurrency = dto.PointsPerUnitCurrency,
            RedemptionValuePerPoint = dto.RedemptionValuePerPoint,
            MinimumRedeemPoints = dto.MinimumRedeemPoints,
            IsActive = true
        };

        var saved = await _settingsRepository.CreateOrUpdateAsync(entity);

        return new LoyaltySettingsDto
        {
            Id = saved.Id,
            PointsPerUnitCurrency = saved.PointsPerUnitCurrency,
            RedemptionValuePerPoint = saved.RedemptionValuePerPoint,
            MinimumRedeemPoints = saved.MinimumRedeemPoints
        };
    }

    public async Task<int> CalculatePointsAsync(decimal amount)
    {
        if (amount <= 0)
            return 0;

        var settings = await _settingsRepository.GetAsync();
        if (settings == null || settings.PointsPerUnitCurrency <= 0)
            return 0;

        var rawPoints = amount * settings.PointsPerUnitCurrency;
        return (int)Math.Floor(rawPoints);
    }

    public async Task<RedeemPointsResponse> RedeemPointsAsync(Guid customerId, int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points to redeem must be greater than zero.");

        var settings = await _settingsRepository.GetAsync();
        if (settings == null)
            throw new InvalidOperationException("Loyalty settings are not configured.");

        if (points < settings.MinimumRedeemPoints)
            throw new InvalidOperationException($"Minimum redeem points is {settings.MinimumRedeemPoints}.");

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            throw new InvalidOperationException("Customer not found.");

        if (customer.LoyaltyPoints < points)
            throw new InvalidOperationException("Insufficient loyalty points.");

        var redemptionAmount = points * settings.RedemptionValuePerPoint;

        customer.LoyaltyPoints -= points;
        customer.UpdatedAt = DateTime.UtcNow;
        await _customerRepository.UpdateAsync(customer);

        return new RedeemPointsResponse
        {
            PointsRedeemed = points,
            RedemptionAmount = redemptionAmount,
            RemainingPoints = customer.LoyaltyPoints
        };
    }
}

