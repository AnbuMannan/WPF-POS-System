using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface IQuotationService
{
    Task<List<QuotationDto>> GetAllAsync(bool includeInactive = false);
    Task<QuotationDto?> GetByIdAsync(Guid id);
    Task<QuotationDto> CreateAsync(CreateQuotationDto dto);
    Task<QuotationDto> UpdateAsync(Guid id, CreateQuotationDto dto);
    Task DisableAsync(Guid id);
    Task<long> ConvertToSaleAsync(Guid quotationId);
}
