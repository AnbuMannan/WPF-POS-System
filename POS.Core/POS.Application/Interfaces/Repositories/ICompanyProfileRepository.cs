using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories
{
    public interface ICompanyProfileRepository
    {
        Task<CompanyProfile?> GetAsync();
        Task<CompanyProfile> CreateOrUpdateAsync(CompanyProfile entity);
    }
}
