using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IQuotationRepository
{
    Task<List<Quotation>> GetAllAsync(bool includeInactive = false);
    Task<Quotation?> GetByIdAsync(Guid id);
    Task<Quotation> AddAsync(Quotation quotation);
    Task<Quotation> UpdateAsync(Quotation quotation);
    Task DisableAsync(Guid id);
    Task<string> GenerateQuotationNumberAsync();
    Task UpdateStatusAsync(Guid id, string status);
}
