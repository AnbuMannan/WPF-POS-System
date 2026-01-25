using POS.Domain.Entities;

public interface IInvoiceService
{
    Task<Guid> CreateInvoiceAsync(Invoice invoice);
}
