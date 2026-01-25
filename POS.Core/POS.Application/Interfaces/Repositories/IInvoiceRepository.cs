using POS.Domain.Entities;

namespace POS.Application.Interfaces.Repositories;

public interface IInvoiceRepository
{
    Task SaveAsync(Invoice invoice);
}
