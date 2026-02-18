using POS.Shared.Models;

namespace POS.Application.Interfaces.Services;

public interface ICustomerPaymentService
{
    Task<List<CustomerBalanceDto>> GetCustomersWithOutstandingAsync();
    Task<CustomerLedgerDto> GetCustomerLedgerAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetOutstandingAsync(Guid customerId);
    Task<CustomerTransactionDto> PayDueAsync(CustomerPaymentRequestDto dto);
    Task RecordSaleTransactionAsync(Guid customerId, long saleId, string billNumber, decimal amount);
    Task RecordReturnTransactionAsync(Guid customerId, int returnId, string returnNumber, decimal amount, string refundMode);
}
