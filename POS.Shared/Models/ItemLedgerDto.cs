namespace POS.Shared.Models;

/// <summary>
/// DTO representing a single item ledger entry for a product
/// </summary>
public class ItemLedgerDto
{
    public Guid Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    
    public DateTime EntryDate { get; set; }
    
    /// <summary>
    /// Transaction type: Sale, Purchase, Return, Adjustment, Opening
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference number (Invoice No, PO No, Adjustment Ref, etc.)
    /// </summary>
    public string ReferenceNo { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference ID for linking to original document
    /// </summary>
    public Guid? ReferenceId { get; set; }
    
    /// <summary>
    /// Quantity received/added (positive)
    /// </summary>
    public decimal InQty { get; set; }
    
    /// <summary>
    /// Quantity issued/removed (positive)
    /// </summary>
    public decimal OutQty { get; set; }
    
    /// <summary>
    /// Running balance after this transaction
    /// </summary>
    public decimal RunningBalance { get; set; }
    
    /// <summary>
    /// Additional remarks/notes
    /// </summary>
    public string? Remarks { get; set; }
    
    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for fetching item ledger
/// </summary>
public class ItemLedgerRequestDto
{
    public long ProductId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Response DTO containing ledger summary and entries
/// </summary>
public class ItemLedgerResponseDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSku { get; set; }
    
    /// <summary>
    /// Opening balance at the start of the period
    /// </summary>
    public decimal OpeningBalance { get; set; }
    
    /// <summary>
    /// Total quantity received during the period
    /// </summary>
    public decimal TotalIn { get; set; }
    
    /// <summary>
    /// Total quantity issued during the period
    /// </summary>
    public decimal TotalOut { get; set; }
    
    /// <summary>
    /// Closing balance at the end of the period
    /// </summary>
    public decimal ClosingBalance { get; set; }
    
    /// <summary>
    /// List of ledger entries
    /// </summary>
    public List<ItemLedgerDto> Entries { get; set; } = new();
}
