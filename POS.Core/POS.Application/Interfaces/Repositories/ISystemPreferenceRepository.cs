using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories
{
    public interface ISystemPreferenceRepository
    {
        Task<SystemPreference?> GetByStoreAsync(int storeCode);
        Task<SystemPreference> CreateOrUpdateAsync(SystemPreference entity);
    }
}