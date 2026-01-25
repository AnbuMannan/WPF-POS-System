namespace POS.Domain.Entities;

public class Invoice
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public bool IsInterState { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTax { get; set; }
    public List<InvoiceItem> Items { get; set; }
}
