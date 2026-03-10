using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories;

public class LoyaltySettingsRepository : ILoyaltySettingsRepository
{
    private readonly PosDbContext _db;

    public LoyaltySettingsRepository(PosDbContext db)
    {
        _db = db;
    }

    public async Task<LoyaltySetting?> GetAsync()
    {
        return await _db.LoyaltySettings.AsNoTracking().FirstOrDefaultAsync(ls => ls.IsActive);
    }

    public async Task<LoyaltySetting> CreateOrUpdateAsync(LoyaltySetting entity)
    {
        var existing = await _db.LoyaltySettings.FirstOrDefaultAsync();

        if (existing == null)
        {
            entity.CreatedAt = DateTime.Now;
            entity.IsActive = true;
            await _db.LoyaltySettings.AddAsync(entity);
        }
        else
        {
            existing.PointsPerUnitCurrency = entity.PointsPerUnitCurrency;
            existing.RedemptionValuePerPoint = entity.RedemptionValuePerPoint;
            existing.MinimumRedeemPoints = entity.MinimumRedeemPoints;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.Now;
            entity = existing;
        }

        await _db.SaveChangesAsync();
        return entity;
    }
}

