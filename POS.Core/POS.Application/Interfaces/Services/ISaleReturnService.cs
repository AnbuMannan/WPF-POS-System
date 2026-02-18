using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ISaleReturnService
{
    Task<List<SaleReturnDto>> GetAllAsync();
    Task<SaleReturnDto?> GetByIdAsync(int id);
    Task<SaleInvoiceForReturnDto?> LookupInvoiceAsync(string billNumber);
    Task<SaleInvoiceForReturnDto?> LookupInvoiceBySaleIdAsync(long saleId);
    Task<SaleReturnDto> CreateReturnAsync(CreateSaleReturnDto dto);
    Task<SaleReturnDto> ProcessReturnAsync(int returnId);
}
