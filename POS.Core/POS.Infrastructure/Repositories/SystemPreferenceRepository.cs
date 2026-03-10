using Microsoft.EntityFrameworkCore;
using POS.Application.Interfaces.Repositories;
using POS.Domain.Entities;
using POS.Infrastructure.Data;

namespace POS.Infrastructure.Repositories
{
    public class SystemPreferenceRepository : ISystemPreferenceRepository
    {
        private readonly PosDbContext _db;

        public SystemPreferenceRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<SystemPreference?> GetByStoreAsync(int storeCode)
        {
            return await _db.SystemPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(sp => sp.StoreCode == storeCode);
        }

        public async Task<SystemPreference> CreateOrUpdateAsync(SystemPreference entity)
        {
            var existing = await _db.SystemPreferences
                .FirstOrDefaultAsync(sp => sp.StoreCode == entity.StoreCode);

            if (existing == null)
            {
                entity.CreatedAt = DateTime.Now;
                await _db.SystemPreferences.AddAsync(entity);
            }
            else
            {
                existing.SidebarIdleTimeoutSeconds = entity.SidebarIdleTimeoutSeconds;
                existing.UpdatedAt = DateTime.Now;
                _db.SystemPreferences.Update(existing);
                entity = existing;
            }

            await _db.SaveChangesAsync();
            return entity;
        }
    }
}