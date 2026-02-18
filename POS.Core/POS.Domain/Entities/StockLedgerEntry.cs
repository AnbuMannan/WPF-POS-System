namespace POS.Domain.Entities;

public class StockLedgerEntry
{
    public Guid StockEntryId { get; set; }
    public long ProductId { get; set; } // Changed from Guid to long to match Product.ProductId
    public decimal Quantity { get; set; }
    public string EntryType { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT, RETURN
    public string ReferenceType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
