namespace POS.Shared.Models;

public class CustomerTransactionDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public string? PaymentMode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerBalanceDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

public class CustomerPaymentRequestDto
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMode { get; set; } = "Cash";
    public string? ReferenceNo { get; set; }
    public string? Remarks { get; set; }
}

public class CustomerLedgerDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public List<CustomerTransactionDto> Entries { get; set; } = new();
}
