using POS.Shared.Models;

namespace POS.Application.Interfaces.Services
{
    public interface ICompanyProfileService
    {
        Task<CompanyProfileDto?> GetAsync();
        Task<CompanyProfileDto> SaveAsync(UpdateCompanyProfileDto dto);
    }
}
