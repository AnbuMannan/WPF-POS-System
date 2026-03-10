using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services
{
    public class SystemPreferenceService : ISystemPreferenceService
    {
        private readonly ISystemPreferenceRepository _repo;

        public SystemPreferenceService(ISystemPreferenceRepository repo)
        {
            _repo = repo;
        }

        public async Task<SystemPreferenceDto?> GetByStoreAsync(int storeCode)
        {
            var entity = await _repo.GetByStoreAsync(storeCode);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<SystemPreferenceDto> UpdateAsync(int storeCode, UpdateSystemPreferenceDto dto)
        {
            if (dto.SidebarIdleTimeoutSeconds <= 0)
                throw new ArgumentException("Sidebar idle timeout must be greater than 0");

            var entity = new SystemPreference
            {
                StoreCode = storeCode,
                SidebarIdleTimeoutSeconds = dto.SidebarIdleTimeoutSeconds
            };

            var updatedEntity = await _repo.CreateOrUpdateAsync(entity);
            return MapToDto(updatedEntity);
        }

        private SystemPreferenceDto MapToDto(SystemPreference entity)
        {
            return new SystemPreferenceDto
            {
                Id = entity.Id,
                StoreCode = entity.StoreCode,
                SidebarIdleTimeoutSeconds = entity.SidebarIdleTimeoutSeconds,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}