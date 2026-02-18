using POS.Application.Exceptions;
using POS.Application.Interfaces.Repositories;
using POS.Application.Interfaces.Services;
using POS.Domain.Entities;
using POS.Shared.Models;

namespace POS.Application.Services;

public class CustomerPaymentService : ICustomerPaymentService
{
    private readonly ICustomerTransactionRepository _transactionRepo;
    private readonly ICustomerRepository _customerRepo;

    public CustomerPaymentService(
        ICustomerTransactionRepository transactionRepo,
        ICustomerRepository customerRepo)
    {
        _transactionRepo = transactionRepo;
        _customerRepo = customerRepo;
    }

    public async Task<List<CustomerBalanceDto>> GetCustomersWithOutstandingAsync()
    {
        var data = await _transactionRepo.GetCustomersWithBalanceAsync();
        return data.Select(d => new CustomerBalanceDto
        {
            CustomerId = d.Customer.Id,
            CustomerName = d.Customer.Name,
            Phone = d.Customer.Phone,
            Email = d.Customer.Email,
            CurrentBalance = d.Balance,
            LastTransactionDate = d.LastTransactionDate
        }).ToList();
    }

    public async Task<CustomerLedgerDto> GetCustomerLedgerAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var customer = await _customerRepo.GetByIdAsync(customerId);
        if (customer == null)
            throw new ValidationException("Customer", "Customer not found.");

        List<CustomerTransaction> transactions;
        if (fromDate.HasValue && toDate.HasValue)
            transactions = await _transactionRepo.GetByCustomerIdAsync(customerId, fromDate.Value, toDate.Value);
        else
            transactions = await _transactionRepo.GetByCustomerIdAsync(customerId);

        var entries = transactions.Select(t => new CustomerTransactionDto
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            TransactionDate = t.TransactionDate,
            TransactionType = t.TransactionType,
            ReferenceId = t.ReferenceId,
            ReferenceNo = t.ReferenceNo,
            DebitAmount = t.DebitAmount,
            CreditAmount = t.CreditAmount,
            Balance = t.Balance,
            Description = t.Description,
            PaymentMode = t.PaymentMode,
            CreatedAt = t.CreatedAt
        }).OrderBy(e => e.TransactionDate).ToList();

        return new CustomerLedgerDto
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            Phone = customer.Phone,
            OpeningBalance = entries.FirstOrDefault()?.Balance ?? 0,
            ClosingBalance = entries.LastOrDefault()?.Balance ?? 0,
            TotalDebit = entries.Sum(e => e.DebitAmount),
            TotalCredit = entries.Sum(e => e.CreditAmount),
            Entries = entries
        };
    }

    public async Task<decimal> GetOutstandingAsync(Guid customerId)
    {
        return await _transactionRepo.GetBalanceAsync(customerId);
    }

    public async Task<CustomerTransactionDto> PayDueAsync(CustomerPaymentRequestDto dto)
    {
        if (dto.Amount <= 0)
            throw new ValidationException("Amount", "Payment amount must be greater than 0.");

        var customer = await _customerRepo.GetByIdAsync(dto.CustomerId);
        if (customer == null)
            throw new ValidationException("Customer", "Customer not found.");

        var currentBalance = await _transactionRepo.GetBalanceAsync(dto.CustomerId);

        var paymentNo = $"CPAY-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";

        var transaction = new CustomerTransaction
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            TransactionDate = DateTime.Now,
            TransactionType = "Payment",
            ReferenceNo = paymentNo,
            DebitAmount = 0,
            CreditAmount = dto.Amount,
            Balance = currentBalance - dto.Amount,
            Description = $"Payment received - {dto.PaymentMode}" + (string.IsNullOrEmpty(dto.Remarks) ? "" : $" - {dto.Remarks}"),
            PaymentMode = dto.PaymentMode,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        var created = await _transactionRepo.AddAsync(transaction);

        return new CustomerTransactionDto
        {
            Id = created.Id,
            CustomerId = created.CustomerId,
            CustomerName = customer.Name,
            TransactionDate = created.TransactionDate,
            TransactionType = created.TransactionType,
            ReferenceNo = created.ReferenceNo,
            DebitAmount = created.DebitAmount,
            CreditAmount = created.CreditAmount,
            Balance = created.Balance,
            Description = created.Description,
            PaymentMode = created.PaymentMode,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task RecordSaleTransactionAsync(Guid customerId, long saleId, string billNumber, decimal amount)
    {
        var currentBalance = await _transactionRepo.GetBalanceAsync(customerId);

        var transaction = new CustomerTransaction
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TransactionDate = DateTime.Now,
            TransactionType = "Sale",
            ReferenceId = null, // Sale uses long ID, not Guid
            ReferenceNo = billNumber,
            DebitAmount = amount,
            CreditAmount = 0,
            Balance = currentBalance + amount,
            Description = $"Sale Invoice: {billNumber}",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _transactionRepo.AddAsync(transaction);
    }

    public async Task RecordReturnTransactionAsync(Guid customerId, int returnId, string returnNumber, decimal amount, string refundMode)
    {
        var currentBalance = await _transactionRepo.GetBalanceAsync(customerId);

        var transaction = new CustomerTransaction
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TransactionDate = DateTime.Now,
            TransactionType = refundMode == "CreditNote" ? "CreditNote" : "Return",
            ReferenceNo = returnNumber,
            DebitAmount = 0,
            CreditAmount = amount,
            Balance = currentBalance - amount,
            Description = $"Sales Return: {returnNumber} ({refundMode})",
            PaymentMode = refundMode,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _transactionRepo.AddAsync(transaction);
    }
}
