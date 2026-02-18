namespace POS.Shared.Models;

/// <summary>
/// DTO for supplier transaction/ledger data
/// </summary>
public class SupplierTransactionDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNo { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for supplier balance summary
/// </summary>
public class SupplierBalanceDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}
