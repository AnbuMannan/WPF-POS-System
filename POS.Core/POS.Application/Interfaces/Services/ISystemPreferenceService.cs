using POS.Shared.Models;

namespace POS.Application.Interfaces.Services
{
    public interface ISystemPreferenceService
    {
        Task<SystemPreferenceDto?> GetByStoreAsync(int storeCode);
        Task<SystemPreferenceDto> UpdateAsync(int storeCode, UpdateSystemPreferenceDto dto);
    }
}