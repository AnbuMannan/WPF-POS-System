namespace POS.Domain.Entities;

public class StockLedgerEntry
{
    public Guid StockEntryId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string EntryType { get; set; } // IN, OUT, ADJUSTMENT, RETURN
    public string ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Remarks { get; set; }
}
