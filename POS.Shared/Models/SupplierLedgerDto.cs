namespace POS.Shared.Models;

/// <summary>
/// DTO for supplier ledger line item
/// </summary>
public class SupplierLedgerEntryDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RunningBalance { get; set; }
}

/// <summary>
/// DTO for complete supplier ledger report with opening/closing balances
/// </summary>
public class SupplierLedgerReportDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Mobile { get; set; }
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    /// <summary>
    /// Balance before the from date (sum of all transactions before FromDate)
    /// </summary>
    public decimal OpeningBalance { get; set; }
    
    /// <summary>
    /// Balance at the end of the report period
    /// </summary>
    public decimal ClosingBalance { get; set; }
    
    /// <summary>
    /// Total debits in the period
    /// </summary>
    public decimal TotalDebit { get; set; }
    
    /// <summary>
    /// Total credits in the period
    /// </summary>
    public decimal TotalCredit { get; set; }
    
    /// <summary>
    /// Ledger entries within the date range
    /// </summary>
    public List<SupplierLedgerEntryDto> Entries { get; set; } = new();
}
